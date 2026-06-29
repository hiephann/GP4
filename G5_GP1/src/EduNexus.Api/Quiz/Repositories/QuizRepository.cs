using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Quiz.Entities;

namespace EduNexus.Api.Quiz.Repositories;

public interface IQuizRepository : IRepository<QuizItem>
{
    Task<List<QuizAttempt>> GetHistoryAsync(Guid studentId, CancellationToken ct = default);
    Task<QuizAttempt?> GetAttemptWithAnswersAsync(Guid attemptId, CancellationToken ct = default);
}

public class QuizRepository : EfRepository<QuizItem>, IQuizRepository
{
    public QuizRepository(EduNexusDbContext db) : base(db) { }

    public Task<List<QuizAttempt>> GetHistoryAsync(Guid studentId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<QuizAttempt?> GetAttemptWithAnswersAsync(Guid attemptId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: Include(Answers)
}
