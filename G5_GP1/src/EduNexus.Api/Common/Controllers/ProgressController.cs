using EduNexus.Api.Common.DTOs;
using EduNexus.Api.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Common.Controllers;

// Màn hình: Personal Progress — FT-12
[ApiController]
[Route("api/students/{studentId:guid}/progress")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progress;

    public ProgressController(IProgressService progress) => _progress = progress;

    [HttpGet("courses/{courseId:guid}")]
    public async Task<ActionResult<PersonalProgressDto>> Get(Guid studentId, Guid courseId, CancellationToken ct)
        => Ok(await _progress.GetPersonalProgressAsync(studentId, courseId, ct));
}
