using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Lesson.Entities;

namespace EduNexus.Api.Lesson.Repositories;

public interface ILessonRepository : IRepository<Entities.Lesson>
{
    Task<List<Entities.Lesson>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task<Entities.Lesson?> GetWithContentsAsync(Guid lessonId, CancellationToken ct = default);
}

public class LessonRepository : EfRepository<Entities.Lesson>, ILessonRepository
{
    public LessonRepository(EduNexusDbContext db) : base(db) { }

    public Task<List<Entities.Lesson>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<Entities.Lesson?> GetWithContentsAsync(Guid lessonId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: Include(Contents)
}
