using EduNexus.Api.Infrastructure;
using EduNexus.Api.Quiz.DTOs;
using EduNexus.Api.Quiz.Repositories;
using Microsoft.EntityFrameworkCore;
using QuestionEntity = EduNexus.Api.Question.Entities.Question;
using QuizEntities = EduNexus.Api.Quiz.Entities;

namespace EduNexus.Api.Quiz.Services;

// FT-07: practice quizzes never enter the official grade book.
public interface IQuizService
{
    Task<List<QuizHistoryItemDto>> GetHistoryAsync(Guid studentId, CancellationToken ct = default);
    Task<QuizTakingDto> CreateAsync(CreateQuizRequest request, CancellationToken ct = default);
    Task<QuizTakingDto?> GetForTakingAsync(Guid quizId, CancellationToken ct = default);
    Task<QuizResultDto> SubmitAsync(Guid quizId, SubmitQuizRequest request, CancellationToken ct = default);
    Task<QuizResultDto?> GetResultAsync(Guid attemptId, CancellationToken ct = default);
}

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizzes;
    private readonly EduNexusDbContext _db;

    public QuizService(IQuizRepository quizzes, EduNexusDbContext db)
    {
        _quizzes = quizzes;
        _db = db;
    }

    public async Task<List<QuizHistoryItemDto>> GetHistoryAsync(Guid studentId, CancellationToken ct = default) =>
        (await _quizzes.GetHistoryAsync(studentId, ct))
            .Select(attempt => new QuizHistoryItemDto(attempt.Id, attempt.QuizId, attempt.SubmittedAt, attempt.Score))
            .ToList();

    public async Task<QuizTakingDto> CreateAsync(CreateQuizRequest request, CancellationToken ct = default)
    {
        if (request.StudentId == Guid.Empty || !await _db.Users.AnyAsync(user => user.Id == request.StudentId && user.IsActive, ct))
            throw new InvalidOperationException("An active student account is required.");
        if (request.QuestionCount is < 1 or > 100)
            throw new InvalidOperationException("Question count must be between 1 and 100.");

        var requestedModules = request.ModuleIds.Distinct().ToList();
        IQueryable<QuestionEntity> candidates = _db.Questions.AsNoTracking()
            .Where(question => question.Status == "Active" && _db.QuestionOptions.Any(option => option.QuestionId == question.Id && option.IsCorrect));

        if (request.CourseId is Guid courseId)
        {
            var courseModuleIds = _db.Modules.Where(module => module.CourseId == courseId).Select(module => module.Id);
            candidates = candidates.Where(question => courseModuleIds.Contains(question.ModuleId));
        }
        if (requestedModules.Count > 0)
            candidates = candidates.Where(question => requestedModules.Contains(question.ModuleId));
        if (!string.IsNullOrWhiteSpace(request.Difficulty))
            candidates = candidates.Where(question => question.Difficulty == request.Difficulty.Trim());

        // NAC-07-a: Take returns all matching questions when fewer than requested exist.
        var selectedIds = await candidates.OrderBy(_ => Guid.NewGuid()).Take(request.QuestionCount)
            .Select(question => question.Id).ToListAsync(ct);
        if (selectedIds.Count == 0)
            throw new InvalidOperationException("No eligible questions were found for the selected scope.");

        var quiz = new QuizEntities.QuizItem
        {
            Id = Guid.NewGuid(), StudentId = request.StudentId, CourseId = request.CourseId,
            Title = "Practice quiz", QuestionCount = selectedIds.Count,
            Difficulty = string.IsNullOrWhiteSpace(request.Difficulty) ? null : request.Difficulty.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Quizzes.Add(quiz);
        _db.QuizQuestions.AddRange(selectedIds.Select((questionId, index) => new QuizEntities.QuizQuestion
        {
            Id = Guid.NewGuid(), QuizId = quiz.Id, QuestionId = questionId, OrderIndex = index
        }));
        await _db.SaveChangesAsync(ct);
        return await GetForTakingAsync(quiz.Id, ct) ?? throw new InvalidOperationException("Quiz could not be loaded.");
    }

    public async Task<QuizTakingDto?> GetForTakingAsync(Guid quizId, CancellationToken ct = default)
    {
        var quiz = await _quizzes.GetQuizWithQuestionsAsync(quizId, ct);
        if (quiz is null) return null;

        var orderedIds = quiz.Questions.OrderBy(link => link.OrderIndex).Select(link => link.QuestionId).ToList();
        var questions = await _db.Questions.AsNoTracking().Include(question => question.Options)
            .Where(question => orderedIds.Contains(question.Id)).ToListAsync(ct);
        var questionById = questions.ToDictionary(question => question.Id);

        // NAC-07-b: no correct-answer flag or explanation is put in this DTO.
        var items = orderedIds.Where(questionById.ContainsKey).Select(questionId =>
        {
            var question = questionById[questionId];
            return new QuizQuestionDto(question.Id, question.Content,
                question.Options.OrderBy(option => option.OrderIndex)
                    .Select(option => new QuizOptionDto(option.Id, option.Content)).ToList());
        }).ToList();
        return new QuizTakingDto(quiz.Id, items);
    }

    public async Task<QuizResultDto> SubmitAsync(Guid quizId, SubmitQuizRequest request, CancellationToken ct = default)
    {
        var quiz = await _quizzes.GetQuizWithQuestionsAsync(quizId, ct)
            ?? throw new InvalidOperationException("Quiz not found.");
        if (quiz.StudentId != request.StudentId)
            throw new UnauthorizedAccessException("This quiz belongs to another student.");
        if (await _db.QuizAttempts.AnyAsync(attempt => attempt.QuizId == quizId && attempt.StudentId == request.StudentId && attempt.SubmittedAt != null, ct))
            throw new InvalidOperationException("This quiz has already been submitted.");

        var questionIds = quiz.Questions.Select(link => link.QuestionId).ToHashSet();
        var submittedAnswers = request.Answers.ToList();
        if (submittedAnswers.GroupBy(answer => answer.QuestionId).Any(group => group.Count() > 1) || submittedAnswers.Any(answer => !questionIds.Contains(answer.QuestionId)))
            throw new InvalidOperationException("The submitted answers do not match this quiz.");

        var selectedByQuestion = submittedAnswers.ToDictionary(answer => answer.QuestionId, answer => answer.SelectedOptionId);
        var options = await _db.QuestionOptions.AsNoTracking().Where(option => questionIds.Contains(option.QuestionId)).ToListAsync(ct);
        var optionsById = options.ToDictionary(option => option.Id);
        foreach (var (questionId, selectedOptionId) in selectedByQuestion)
            if (selectedOptionId is Guid optionId && (!optionsById.TryGetValue(optionId, out var option) || option.QuestionId != questionId))
                throw new InvalidOperationException("An answer contains an invalid option.");

        var attempt = new QuizEntities.QuizAttempt
        {
            Id = Guid.NewGuid(), QuizId = quizId, StudentId = request.StudentId,
            StartedAt = DateTime.UtcNow, SubmittedAt = DateTime.UtcNow
        };
        var answers = questionIds.Select(questionId =>
        {
            var selectedOptionId = selectedByQuestion.GetValueOrDefault(questionId);
            var isCorrect = selectedOptionId is Guid optionId && optionsById[optionId].IsCorrect;
            return new QuizEntities.QuizAttemptAnswer
            {
                Id = Guid.NewGuid(), AttemptId = attempt.Id, QuestionId = questionId,
                SelectedOptionId = selectedOptionId, IsCorrect = isCorrect
            };
        }).ToList();
        attempt.Answers = answers;
        attempt.Score = Math.Round(answers.Count(answer => answer.IsCorrect == true) * 10m / questionIds.Count, 2);
        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync(ct);
        return await BuildResultAsync(attempt, ct);
    }

    public async Task<QuizResultDto?> GetResultAsync(Guid attemptId, CancellationToken ct = default)
    {
        var attempt = await _quizzes.GetAttemptWithAnswersAsync(attemptId, ct);
        return attempt is null || attempt.SubmittedAt is null ? null : await BuildResultAsync(attempt, ct);
    }

    private async Task<QuizResultDto> BuildResultAsync(QuizEntities.QuizAttempt attempt, CancellationToken ct)
    {
        var questionIds = attempt.Answers.Select(answer => answer.QuestionId).ToList();
        var questions = await _db.Questions.AsNoTracking().Where(question => questionIds.Contains(question.Id))
            .ToDictionaryAsync(question => question.Id, ct);
        var correctOptions = await _db.QuestionOptions.AsNoTracking()
            .Where(option => questionIds.Contains(option.QuestionId) && option.IsCorrect)
            .ToDictionaryAsync(option => option.QuestionId, ct);

        var items = attempt.Answers.Select(answer =>
        {
            var question = questions[answer.QuestionId];
            return new QuizReviewItemDto(question.Id, question.Content, answer.SelectedOptionId,
                correctOptions[question.Id].Id, answer.IsCorrect == true, question.Explanation);
        }).ToList();
        return new QuizResultDto(attempt.Id, attempt.Score ?? 0, items.Count(item => item.IsCorrect), items.Count, items);
    }
}
