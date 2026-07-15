using EduNexus.Api.Common.Entities;
using EduNexus.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Common.Repositories;

public interface ICourseRepository : IRepository<Course>
{
    Task<Course?> GetWithModulesAsync(Guid courseId, CancellationToken ct = default);
}

public class CourseRepository : EfRepository<Course>, ICourseRepository
{
    public CourseRepository(EduNexusDbContext db) : base(db) { }

    public Task<Course?> GetWithModulesAsync(Guid courseId, CancellationToken ct = default)
        => Db.Courses.AsNoTracking().Include(c => c.Modules)
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);
}
