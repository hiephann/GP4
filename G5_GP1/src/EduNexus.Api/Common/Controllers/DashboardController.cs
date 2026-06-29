using EduNexus.Api.Common.DTOs;
using EduNexus.Api.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Common.Controllers;

// Màn hình: Student Dashboard
[ApiController]
[Route("api/students/{studentId:guid}/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(Guid studentId, CancellationToken ct)
        => Ok(await _dashboard.GetStudentDashboardAsync(studentId, ct));
}
