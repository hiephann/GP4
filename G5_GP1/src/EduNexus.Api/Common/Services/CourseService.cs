using EduNexus.Api.Common.DTOs;
using EduNexus.Api.Common.Repositories;

namespace EduNexus.Api.Common.Services;

// FT-01 — Course List & Course Structure
public interface ICourseService
{
    Task<List<CourseListItemDto>> GetCoursesForSmeAsync(Guid smeId, CancellationToken ct = default);
    Task<CourseStructureDto?> GetStructureAsync(Guid courseId, CancellationToken ct = default);
    Task<Guid> CreateCourseAsync(string title, Guid smeId, CancellationToken ct = default);
    Task PublishAsync(Guid courseId, CancellationToken ct = default);
}

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;

    public CourseService(ICourseRepository courses) => _courses = courses;

    public Task<List<CourseListItemDto>> GetCoursesForSmeAsync(Guid smeId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<CourseStructureDto?> GetStructureAsync(Guid courseId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: module + đếm lessons/questions/flashcards/assignments

    public Task<Guid> CreateCourseAsync(string title, Guid smeId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task PublishAsync(Guid courseId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: AC-01c phải có >=1 module
}
