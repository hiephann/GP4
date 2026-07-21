using System.Text.Json;
using EduNexus.Api.Assignment.DTOs;
using EduNexus.Api.Assignment.Entities;
using EduNexus.Api.Assignment.Repositories;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Infrastructure.Ai;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Assignment.Services;

public interface IAssignmentService
{
    Task<List<AssignmentListItemDto>> GetListAsync(Guid moduleId, CancellationToken ct = default);
    Task<AssignmentDetailDto?> GetDetailAsync(Guid assignmentId, CancellationToken ct = default);
    Task<Guid> UpsertAsync(UpsertAssignmentRequest request, CancellationToken ct = default);
    Task<Guid> SubmitAsync(Guid assignmentId, SubmitAssignmentRequest request, CancellationToken ct = default);
    Task<SubmissionResultDto?> GetResultAsync(Guid submissionId, CancellationToken ct = default);
    Task ConfirmGradeAsync(Guid submissionId, ConfirmGradeRequest request, CancellationToken ct = default);
}

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignments;
    private readonly EduNexusDbContext _db;
    private readonly IAiContentService _ai;
    private readonly ILogger<AssignmentService> _logger;
    public AssignmentService(IAssignmentRepository assignments, EduNexusDbContext db, IAiContentService ai, ILogger<AssignmentService> logger)
        => (_assignments, _db, _ai, _logger) = (assignments, db, ai, logger);

    public async Task<List<AssignmentListItemDto>> GetListAsync(Guid moduleId, CancellationToken ct = default) =>
        (await _assignments.GetByModuleAsync(moduleId, ct)).Select(item => new AssignmentListItemDto(item.Id, item.Title, item.DueDate, item.Status)).ToList();

    public async Task<AssignmentDetailDto?> GetDetailAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var item = await _assignments.GetWithCriteriaAsync(assignmentId, ct);
        return item is null ? null : new AssignmentDetailDto(item.Id, item.Title, item.PromptMarkdown, item.DueDate, item.MaxChars, item.Status,
            item.Criteria.OrderBy(c => c.OrderIndex).Select(c => new RubricCriterionDto(c.Id, c.Name, c.Weight, c.MaxScore)).ToList());
    }

    public async Task<Guid> UpsertAsync(UpsertAssignmentRequest request, CancellationToken ct = default)
    {
        var criteria = request.Criteria.ToList();
        if (request.ModuleId == Guid.Empty || string.IsNullOrWhiteSpace(request.Title) || criteria.Count == 0 || criteria.Sum(c => c.Weight) != 100m || criteria.Any(c => string.IsNullOrWhiteSpace(c.Name) || c.Weight <= 0 || c.MaxScore <= 0))
            throw new InvalidOperationException("Assignment requires a title and rubric weights totaling exactly 100%.");
        var item = new AssignmentItem { Id = Guid.NewGuid(), ModuleId = request.ModuleId, Title = request.Title.Trim(), PromptMarkdown = request.PromptMarkdown, DueDate = request.DueDate, Status = "Draft", CreatedAt = DateTime.UtcNow };
        item.Criteria = criteria.Select((criterion, index) => new RubricCriterion { Id = Guid.NewGuid(), AssignmentId = item.Id, Name = criterion.Name.Trim(), Weight = criterion.Weight, MaxScore = criterion.MaxScore, OrderIndex = index }).ToList();
        _db.Assignments.Add(item); await _db.SaveChangesAsync(ct); return item.Id;
    }

    public async Task<Guid> SubmitAsync(Guid assignmentId, SubmitAssignmentRequest request, CancellationToken ct = default)
    {
        var assignment = await _assignments.GetWithCriteriaAsync(assignmentId, ct) ?? throw new InvalidOperationException("Assignment not found.");
        if (assignment.Status != "Published" || assignment.DueDate < DateTime.UtcNow || string.IsNullOrWhiteSpace(request.ContentText) || request.ContentText.Length > assignment.MaxChars)
            throw new InvalidOperationException("This submission does not satisfy assignment requirements.");
        if (await _db.Submissions.AnyAsync(s => s.AssignmentId == assignmentId && s.StudentId == request.StudentId, ct)) throw new InvalidOperationException("Only one submission is allowed.");
        var submission = new Submission { Id = Guid.NewGuid(), AssignmentId = assignmentId, StudentId = request.StudentId, ContentText = request.ContentText.Trim(), SubmittedAt = DateTime.UtcNow, Status = "Submitted" };
        _db.Submissions.Add(submission); await _db.SaveChangesAsync(ct);
        var prompt = JsonSerializer.Serialize(new { assignment = assignment.Title, rubric = assignment.Criteria.Select(c => new { criterionId = c.Id, name = c.Name, maxScore = c.MaxScore }), submission = submission.ContentText });
        try
        {
            var ai = await _ai.GradeAssignmentAsync(prompt, ct);
            using var doc = JsonDocument.Parse(ai.Text);
            var scores = doc.RootElement.GetProperty("criteria").EnumerateArray().Select(x => new { Id = x.GetProperty("criterionId").GetGuid(), Score = x.GetProperty("score").GetDecimal(), Comment = x.GetProperty("comment").GetString() }).ToDictionary(x => x.Id);
            var criterionScores = assignment.Criteria
                .Select(criterion => new SubmissionCriterionScore
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    CriterionId = criterion.Id,
                    AiScore = scores.TryGetValue(criterion.Id, out var result) ? Math.Clamp(result.Score, 0, criterion.MaxScore) : 0,
                    Comment = scores.TryGetValue(criterion.Id, out result) ? result.Comment : "AI did not return this criterion."
                })
                .ToList();

            // The submission was persisted before the AI request. Register the
            // newly created child rows explicitly so EF always emits INSERTs.
            _db.SubmissionCriterionScores.AddRange(criterionScores);
            submission.AiTotalScore = criterionScores.Sum(score => score.AiScore ?? 0);
            submission.Status = "AiGraded";
        }
        catch (Exception ex) when (ct.IsCancellationRequested == false)
        {
            _logger.LogWarning(ex, "AI grading failed for submission {SubmissionId}; preserving it for teacher review.", submission.Id);
            submission.Status = "Submitted";
        }
        await _db.SaveChangesAsync(ct); return submission.Id;
    }

    public async Task<SubmissionResultDto?> GetResultAsync(Guid submissionId, CancellationToken ct = default)
    {
        var submission = await _db.Submissions.AsNoTracking().Include(s => s.CriterionScores).FirstOrDefaultAsync(s => s.Id == submissionId, ct);
        if (submission is null) return null;
        var names = await _db.RubricCriteria.Where(c => submission.CriterionScores.Select(s => s.CriterionId).Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, ct);
        return new SubmissionResultDto(submission.Id, submission.Status, submission.FinalScore, submission.TeacherComment, submission.CriterionScores.Select(s => new CriterionScoreDto(s.CriterionId, names.GetValueOrDefault(s.CriterionId, "Criterion"), s.FinalScore, s.Comment)).ToList());
    }

    public async Task ConfirmGradeAsync(Guid submissionId, ConfirmGradeRequest request, CancellationToken ct = default)
    {
        var submission = await _db.Submissions.Include(s => s.CriterionScores).FirstOrDefaultAsync(s => s.Id == submissionId, ct) ?? throw new InvalidOperationException("Submission not found.");
        foreach (var score in request.CriterionScores) { var target = submission.CriterionScores.FirstOrDefault(s => s.CriterionId == score.CriterionId); if (target is not null) { target.FinalScore = score.Score; target.Comment = score.Comment; } }
        submission.FinalScore = request.FinalScore; submission.TeacherComment = request.TeacherComment; submission.ConfirmedById = request.TeacherId; submission.ConfirmedAt = DateTime.UtcNow; submission.Status = "Confirmed"; await _db.SaveChangesAsync(ct);
    }
}
