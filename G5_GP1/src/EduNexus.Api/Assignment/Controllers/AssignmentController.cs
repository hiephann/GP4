using EduNexus.Api.Assignment.DTOs;
using EduNexus.Api.Assignment.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Assignment.Controllers;

// Màn hình: Assignment List, Detail, Submit, Result — FT-05, FT-08
[ApiController]
[Route("api/assignments")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignments;

    public AssignmentController(IAssignmentService assignments) => _assignments = assignments;

    // Assignment List
    [HttpGet]
    public async Task<ActionResult<List<AssignmentListItemDto>>> GetList([FromQuery] Guid moduleId, CancellationToken ct)
        => Ok(await _assignments.GetListAsync(moduleId, ct));

    // Assignment Detail
    [HttpGet("{assignmentId:guid}")]
    public async Task<ActionResult<AssignmentDetailDto>> GetDetail(Guid assignmentId, CancellationToken ct)
    {
        var dto = await _assignments.GetDetailAsync(assignmentId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Upsert(UpsertAssignmentRequest request, CancellationToken ct)
        => Ok(await _assignments.UpsertAsync(request, ct));

    // Assignment Submit
    [HttpPost("{assignmentId:guid}/submissions")]
    public async Task<ActionResult<Guid>> Submit(Guid assignmentId, SubmitAssignmentRequest request, CancellationToken ct)
        => Ok(await _assignments.SubmitAsync(assignmentId, request, ct));

    // Assignment Result
    [HttpGet("submissions/{submissionId:guid}/result")]
    public async Task<ActionResult<SubmissionResultDto>> GetResult(Guid submissionId, CancellationToken ct)
    {
        var dto = await _assignments.GetResultAsync(submissionId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    // Giảng viên xác nhận điểm
    [HttpPost("submissions/{submissionId:guid}/confirm-grade")]
    public async Task<IActionResult> ConfirmGrade(Guid submissionId, ConfirmGradeRequest request, CancellationToken ct)
    {
        await _assignments.ConfirmGradeAsync(submissionId, request, ct);
        return NoContent();
    }
}
