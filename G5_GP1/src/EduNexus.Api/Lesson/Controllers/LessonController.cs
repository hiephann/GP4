using EduNexus.Api.Lesson.DTOs;
using EduNexus.Api.Lesson.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Lesson.Controllers;

// Màn hình: Lesson Editor, Lesson View, AI Lesson Staging, Lesson Text Extract — FT-02, FT-06
[ApiController]
[Route("api/lessons")]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessons;

    public LessonController(ILessonService lessons) => _lessons = lessons;

    // Lesson Editor
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateLessonRequest request, CancellationToken ct)
        => Ok(await _lessons.CreateLessonAsync(request, ct));

    [HttpPut("{lessonId:guid}/contents")]
    public async Task<ActionResult<Guid>> UpsertContent(Guid lessonId, UpsertLessonContentRequest request, CancellationToken ct)
        => Ok(await _lessons.UpsertContentAsync(lessonId, request, ct));

    // Lesson View
    [HttpGet("{lessonId:guid}")]
    public async Task<ActionResult<LessonViewDto>> View(Guid lessonId, CancellationToken ct)
    {
        var dto = await _lessons.GetForViewAsync(lessonId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{lessonId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid lessonId, [FromQuery] Guid studentId, CancellationToken ct)
    {
        await _lessons.MarkCompletedAsync(lessonId, studentId, ct);
        return NoContent();
    }

    // AI Lesson Staging
    [HttpPost("ai/generate")]
    public async Task<ActionResult<AiLessonDraftDto>> GenerateDraft(GenerateLessonRequest request, [FromQuery] Guid smeId, CancellationToken ct)
        => Ok(await _lessons.GenerateDraftAsync(request, smeId, ct));

    [HttpPost("ai/drafts/{draftId:guid}/approve")]
    public async Task<IActionResult> ApproveDraft(Guid draftId, CancellationToken ct)
    {
        await _lessons.ApproveDraftAsync(draftId, ct);
        return NoContent();
    }

    // Lesson Text Extract (YouTube transcript -> AI summary)
    [HttpPost("ai/extract-summary")]
    public async Task<ActionResult<LessonSummaryDto>> ExtractSummary(ExtractTranscriptRequest request, CancellationToken ct)
        => Ok(await _lessons.ExtractAndSummarizeAsync(request, ct));
}
