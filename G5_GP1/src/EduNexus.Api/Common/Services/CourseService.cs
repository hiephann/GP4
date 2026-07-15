using EduNexus.Api.Common.DTOs;
using EduNexus.Api.Common.Entities;
using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Common.Services;

public interface ICourseService
{
    Task<List<CourseListItemDto>> GetCoursesForUserAsync(Guid userId, CancellationToken ct = default);
    Task<CourseStructureDto?> GetStructureAsync(Guid courseId, Guid actorId, CancellationToken ct = default);
    Task<Guid> CreateCourseAsync(string title, Guid smeId, CancellationToken ct = default);
    Task PublishAsync(Guid courseId, Guid actorId, CancellationToken ct = default);
}

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly EduNexusDbContext _db;

    public CourseService(ICourseRepository courses, EduNexusDbContext db)
    {
        _courses = courses;
        _db = db;
    }

    public async Task<List<CourseListItemDto>> GetCoursesForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var roles = await GetRoleNamesAsync(userId, ct);
        IQueryable<Course> query = _db.Courses.AsNoTracking();
        if (!roles.Contains("Admin"))
        {
            if (roles.Contains("SME"))
                query = query.Where(c => c.OwnerSmeId == userId);
            else if (roles.Contains("CourseManager"))
            {
                var groupIds = _db.CourseGroupManagers.Where(x => x.UserId == userId).Select(x => x.CourseGroupId);
                query = query.Where(c => _db.CourseGroupCourses.Any(x => x.CourseId == c.Id && groupIds.Contains(x.CourseGroupId)));
            }
            else return new List<CourseListItemDto>();
        }
        return await query.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Select(c => new CourseListItemDto(c.Id, c.Title, c.Status, c.IsVisible)).ToListAsync(ct);
    }

    public async Task<CourseStructureDto?> GetStructureAsync(Guid courseId, Guid actorId, CancellationToken ct = default)
    {
        var course = await _courses.GetWithModulesAsync(courseId, ct);
        if (course is null) return null;

        if (!await CanManageCourseAsync(course, actorId, ct))
            throw new UnauthorizedAccessException("You do not have permission to access this course.");

        var moduleIds = course.Modules.Select(x => x.Id).ToList();
        var lessonCounts = await _db.Lessons.Where(item => moduleIds.Contains(item.ModuleId))
            .GroupBy(item => item.ModuleId).ToDictionaryAsync(group => group.Key, group => group.Count(), ct);
        var questionCounts = await _db.Questions.Where(item => moduleIds.Contains(item.ModuleId))
            .GroupBy(item => item.ModuleId).ToDictionaryAsync(group => group.Key, group => group.Count(), ct);
        var flashcardCounts = await _db.Flashcards.Where(item => moduleIds.Contains(item.ModuleId))
            .GroupBy(item => item.ModuleId).ToDictionaryAsync(group => group.Key, group => group.Count(), ct);
        var assignmentCounts = await _db.Assignments.Where(item => moduleIds.Contains(item.ModuleId))
            .GroupBy(item => item.ModuleId).ToDictionaryAsync(group => group.Key, group => group.Count(), ct);

        var modules = course.Modules.OrderBy(x => x.OrderIndex)
            .Select(module => new ModuleNodeDto(module.Id, module.Title, module.OrderIndex,
                lessonCounts.GetValueOrDefault(module.Id), questionCounts.GetValueOrDefault(module.Id),
                flashcardCounts.GetValueOrDefault(module.Id), assignmentCounts.GetValueOrDefault(module.Id)))
            .ToList();
        return new CourseStructureDto(course.Id, course.Title, modules);
    }

    public async Task<Guid> CreateCourseAsync(string title, Guid smeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 250)
            throw new InvalidOperationException("Course title is required and must not exceed 250 characters.");
        if (!(await GetRoleNamesAsync(smeId, ct)).Overlaps(new[] { "SME", "Admin" }))
            throw new UnauthorizedAccessException("Only SME or Admin can create a course.");
        var course = new Course { Id = Guid.NewGuid(), Title = title.Trim(), OwnerSmeId = smeId, Status = "Draft", IsVisible = true, Version = 1, CreatedAt = DateTime.UtcNow };
        await _courses.AddAsync(course, ct);
        await _courses.SaveChangesAsync(ct);
        return course.Id;
    }

    public async Task PublishAsync(Guid courseId, Guid actorId, CancellationToken ct = default)
    {
        var course = await _db.Courses.Include(x => x.Modules).FirstOrDefaultAsync(x => x.Id == courseId, ct)
            ?? throw new InvalidOperationException("Course not found.");
        if (!await CanManageCourseAsync(course, actorId, ct))
            throw new UnauthorizedAccessException("You do not have permission to publish this course.");
        if (!course.Modules.Any()) throw new InvalidOperationException("A course must contain at least one module before publishing.");
        course.Status = "Published";
        course.IsVisible = true;
        course.PublishedAt = course.UpdatedAt = DateTime.UtcNow;
        course.Version++;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<HashSet<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct) =>
        (await _db.UserRoles.Where(x => x.UserId == userId).Join(_db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name).ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);

    private async Task<bool> CanManageCourseAsync(Course course, Guid actorId, CancellationToken ct)
    {
        var roles = await GetRoleNamesAsync(actorId, ct);
        if (roles.Contains("Admin") || (roles.Contains("SME") && course.OwnerSmeId == actorId)) return true;
        if (!roles.Contains("CourseManager")) return false;

        return await _db.CourseGroupCourses.AnyAsync(groupCourse =>
            groupCourse.CourseId == course.Id && _db.CourseGroupManagers.Any(manager =>
                manager.CourseGroupId == groupCourse.CourseGroupId && manager.UserId == actorId), ct);
    }
}
