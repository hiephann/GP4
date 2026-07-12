using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Question.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Question.Repositories;

public interface IQuestionRepository : IRepository<Entities.Question>
{
    Task<List<Entities.Question>> SearchAsync(QuestionFilter filter, CancellationToken ct = default);
    Task<Entities.Question?> GetWithOptionsAsync(Guid questionId, CancellationToken ct = default);
}

public class QuestionRepository : EfRepository<Entities.Question>, IQuestionRepository
{
    private readonly EduNexusDbContext _db;

    public QuestionRepository(EduNexusDbContext db) : base(db)
    {
        _db = db; 
    }

    public async Task<List<Entities.Question>> SearchAsync(QuestionFilter filter, CancellationToken ct = default)
    {
 
        var query = _db.Questions.AsQueryable();

        if (filter.ModuleId.HasValue)
            query = query.Where(q => q.ModuleId == filter.ModuleId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Difficulty))
            query = query.Where(q => q.Difficulty == filter.Difficulty);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(q => q.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(q => q.Content.Contains(filter.Keyword));

        return await query.OrderByDescending(q => q.CreatedAt).ToListAsync(ct);
    }

    public async Task<Entities.Question?> GetWithOptionsAsync(Guid questionId, CancellationToken ct = default)
    {
        return await _db.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId, ct);
    }
}