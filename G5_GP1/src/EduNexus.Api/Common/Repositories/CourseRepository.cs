using EduNexus.Api.Common.Entities;
using EduNexus.Api.Infrastructure;

namespace EduNexus.Api.Common.Repositories;

public interface ICourseRepository : IRepository<Course>
{
    // TODO: truy vấn riêng cho khóa học (theo SME, theo trạng thái...)
    Task<List<Course>> GetBySmeAsync(Guid smeId, CancellationToken ct = default);
}

public class CourseRepository : EfRepository<Course>, ICourseRepository
{
    public CourseRepository(EduNexusDbContext db) : base(db) { }

    public Task<List<Course>> GetBySmeAsync(Guid smeId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO
}
