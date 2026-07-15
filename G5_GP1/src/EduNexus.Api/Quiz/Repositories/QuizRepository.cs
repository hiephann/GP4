using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Quiz.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Quiz.Repositories;

public interface IQuizRepository : IRepository<QuizItem>
{
    Task<List<QuizAttempt>> GetHistoryAsync(Guid studentId, CancellationToken ct = default);
    Task<QuizAttempt?> GetAttemptWithAnswersAsync(Guid attemptId, CancellationToken ct = default);
    Task<QuizItem?> GetQuizWithQuestionsAsync(Guid quizId, CancellationToken ct = default);
}

public class QuizRepository : EfRepository<QuizItem>, IQuizRepository
{
    public QuizRepository(EduNexusDbContext db) : base(db) { }

    public Task<List<QuizAttempt>> GetHistoryAsync(Guid studentId, CancellationToken ct = default) =>
        Db.QuizAttempts.AsNoTracking()
            .Where(attempt => attempt.StudentId == studentId && attempt.SubmittedAt != null)
            .OrderByDescending(attempt => attempt.SubmittedAt)
            .ToListAsync(ct);

    public Task<QuizAttempt?> GetAttemptWithAnswersAsync(Guid attemptId, CancellationToken ct = default) =>
        Db.QuizAttempts.AsNoTracking().Include(attempt => attempt.Answers)
            .FirstOrDefaultAsync(attempt => attempt.Id == attemptId, ct);

    public Task<QuizItem?> GetQuizWithQuestionsAsync(Guid quizId, CancellationToken ct = default) =>
        Db.Quizzes.AsNoTracking().Include(quiz => quiz.Questions)
            .FirstOrDefaultAsync(quiz => quiz.Id == quizId, ct);
}
