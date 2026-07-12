using EduNexus.Api.Infrastructure;
using EduNexus.Api.Question.DTOs;
using EduNexus.Api.Question.Entities;
using EduNexus.Api.Question.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Question.Services;

public interface IQuestionService
{
    Task<List<QuestionListItemDto>> SearchAsync(QuestionFilter filter, CancellationToken ct = default);
    Task<QuestionDetailDto?> GetDetailAsync(Guid questionId, CancellationToken ct = default);
    Task<Guid> UpsertAsync(UpsertQuestionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid questionId, CancellationToken ct = default);
    Task<ImportResultDto> ImportExcelAsync(Guid moduleId, IFormFile file, CancellationToken ct = default);
    Task<AiQuestionDraftDto> GenerateDraftAsync(GenerateQuestionRequest request, Guid smeId, CancellationToken ct = default);
    Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default);
}

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questions;
    private readonly EduNexusDbContext _db;

    public QuestionService(IQuestionRepository questions, EduNexusDbContext db)
    {
        _questions = questions;
        _db = db;
    }

    public async Task<List<QuestionListItemDto>> SearchAsync(QuestionFilter filter, CancellationToken ct = default)
    {
        var entities = await _questions.SearchAsync(filter, ct);
        return entities.Select(q => new QuestionListItemDto(q.Id, q.Content, q.Difficulty, q.Status)).ToList();
    }

    public async Task<QuestionDetailDto?> GetDetailAsync(Guid questionId, CancellationToken ct = default)
    {
        var q = await _questions.GetWithOptionsAsync(questionId, ct);
        if (q == null) return null;

        var optionsDto = q.Options.OrderBy(o => o.OrderIndex)
            .Select(o => new QuestionOptionDto(o.Id, o.Content, o.IsCorrect, o.OrderIndex));

        return new QuestionDetailDto(q.Id, q.ModuleId, q.Content, q.Explanation, q.Difficulty, q.Status, optionsDto);
    }

    public async Task<Guid> UpsertAsync(UpsertQuestionRequest request, CancellationToken ct = default)
    {
        var optionsList = request.Options.ToList();

        // Business Rule AC-03a validation
        if (optionsList.Count < 2)
            throw new ArgumentException("Cần ít nhất 2 đáp án.");
        if (optionsList.Count(o => o.IsCorrect) != 1)
            throw new ArgumentException("Phải có đúng 1 đáp án đúng.");

        var question = new Entities.Question
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            Content = request.Content.Trim(),
            Explanation = request.Explanation,
            Difficulty = request.Difficulty,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            Options = optionsList.Select(o => new QuestionOption
            {
                Id = Guid.NewGuid(),
                Content = o.Content.Trim(),
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).ToList()
        };

        await _db.Questions.AddAsync(question, ct);
        await _db.SaveChangesAsync(ct);
        return question.Id;
    }

    public async Task DeleteAsync(Guid questionId, CancellationToken ct = default)
    {
        var question = await _db.Questions.FindAsync(new object[] { questionId }, ct);
        if (question != null)
        {
            question.Status = "Archived";
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<ImportResultDto> ImportExcelAsync(Guid moduleId, IFormFile file, CancellationToken ct = default)
    {
        // TODO: Cần cài đặt thư viện EPPlus hoặc ClosedXML để đọc file stream ở đây.
        // Tạm thời trả về Mock result để không văng Exception
        var errors = new List<ImportRowResult>();
        return new ImportResultDto(0, 0, 0, errors);
    }

    public async Task<AiQuestionDraftDto> GenerateDraftAsync(GenerateQuestionRequest request, Guid smeId, CancellationToken ct = default)
    {
        var draft = new AiQuestionDraft
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            CreatedById = smeId,
            SourceText = request.SourceText,
            GeneratedJson = "[{\"q\":\"Câu hỏi AI sinh ra?\",\"options\":[\"A\",\"B\",\"C\",\"D\"],\"answer\":0}]",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _db.AiQuestionDrafts.AddAsync(draft, ct);
        await _db.SaveChangesAsync(ct);

        return new AiQuestionDraftDto(draft.Id, draft.GeneratedJson, draft.Status);
    }

    public async Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default)
    {
        var draft = await _db.AiQuestionDrafts.FindAsync(new object[] { draftId }, ct);
        if (draft != null && draft.Status == "Pending")
        {
            draft.Status = "Approved";
            await _db.SaveChangesAsync(ct);
            // Sau này bạn có thể bổ sung đoạn code parse JSON từ bản nháp
            // rồi Insert vào bảng Questions tại đây.
        }
    }
}