using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Question.DTOs;

namespace EduNexus.Api.Question.Repositories;

public interface IQuestionRepository : IRepository<Entities.Question>
{
    Task<List<Entities.Question>> SearchAsync(QuestionFilter filter, CancellationToken ct = default);
    Task<Entities.Question?> GetWithOptionsAsync(Guid questionId, CancellationToken ct = default);
}

public class QuestionRepository : EfRepository<Entities.Question>, IQuestionRepository
{
    public QuestionRepository(EduNexusDbContext db) : base(db) { }

    public Task<List<Entities.Question>> SearchAsync(QuestionFilter filter, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: lọc theo module/độ khó/trạng thái + tìm kiếm nội dung

    public Task<Entities.Question?> GetWithOptionsAsync(Guid questionId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: Include(Options)
}
