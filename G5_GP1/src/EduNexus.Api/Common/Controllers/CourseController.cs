using EduNexus.Api.Common.DTOs;
using EduNexus.Api.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Common.Controllers;

// Màn hình: Course List & Course Structure — FT-01
[ApiController]
[Route("api/courses")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courses;

    public CourseController(ICourseService courses) => _courses = courses;

    // Course List — danh sách khóa học của SME
    [HttpGet]
    public async Task<ActionResult<List<CourseListItemDto>>> GetForSme([FromQuery] Guid smeId, CancellationToken ct)
        => Ok(await _courses.GetCoursesForSmeAsync(smeId, ct));

    // Course Structure — module list & contents
    [HttpGet("{courseId:guid}/structure")]
    public async Task<ActionResult<CourseStructureDto>> GetStructure(Guid courseId, CancellationToken ct)
    {
        var dto = await _courses.GetStructureAsync(courseId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromQuery] string title, [FromQuery] Guid smeId, CancellationToken ct)
        => Ok(await _courses.CreateCourseAsync(title, smeId, ct));

    [HttpPost("{courseId:guid}/publish")]
    public async Task<IActionResult> Publish(Guid courseId, CancellationToken ct)
    {
        await _courses.PublishAsync(courseId, ct);
        return NoContent();
    }
}
