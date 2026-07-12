using EduNexus.Api.Lesson.DTOs;
using EduNexus.Api.Lesson.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Lesson.Controllers;

// Màn hình: Lesson Editor, Lesson View, AI Lesson Staging, Lesson Text Extract — FT-02, FT-06
[ApiController]
[Route("api/lessons")]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessons;

    public LessonController(ILessonService lessons) => _lessons = lessons;

    // ---------------------------------------------------------------- Lesson Editor

    [HttpGet("modules")]
    public async Task<ActionResult<List<ModuleOptionDto>>> Modules(CancellationToken ct)
        => Ok(await _lessons.GetModuleOptionsAsync(ct));

    [HttpGet("by-module/{moduleId:guid}")]
    public async Task<ActionResult<List<LessonListItemDto>>> ByModule(Guid moduleId, CancellationToken ct)
        => Ok(await _lessons.GetLessonsByModuleAsync(moduleId, ct));

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateLessonRequest request, CancellationToken ct)
        => Ok(await _lessons.CreateLessonAsync(request, ct));

    [HttpGet("{lessonId:guid}/contents")]
    public async Task<ActionResult<List<LessonContentDto>>> Contents(Guid lessonId, CancellationToken ct)
        => Ok(await _lessons.GetContentsAsync(lessonId, ct));

    [HttpPut("{lessonId:guid}/contents")]
    public async Task<ActionResult<Guid>> UpsertContent(Guid lessonId, UpsertLessonContentRequest request, CancellationToken ct)
        => Ok(await _lessons.UpsertContentAsync(lessonId, request, ct));

    [HttpDelete("contents/{contentId:guid}")]
    public async Task<IActionResult> DeleteContent(Guid contentId, CancellationToken ct)
    {
        await _lessons.DeleteContentAsync(contentId, ct);
        return NoContent();
    }

    // ---------------------------------------------------------------- Lesson View

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

    // ---------------------------------------------------------------- AI Lesson Staging

    [HttpGet("ai/drafts")]
    public async Task<ActionResult<List<AiLessonDraftDto>>> Drafts(CancellationToken ct)
        => Ok(await _lessons.GetDraftsAsync(ct));

    [HttpPost("ai/generate")]
    public async Task<ActionResult<AiLessonDraftDto>> GenerateDraft(GenerateLessonRequest request, [FromQuery] Guid smeId, CancellationToken ct)
        => Ok(await _lessons.GenerateDraftAsync(request, smeId, ct));

    [HttpPut("ai/drafts/{draftId:guid}")]
    public async Task<IActionResult> UpdateDraft(Guid draftId, UpdateDraftRequest request, CancellationToken ct)
    {
        await _lessons.UpdateDraftTextAsync(draftId, request.GeneratedText, ct);
        return NoContent();
    }

    [HttpPost("ai/drafts/{draftId:guid}/approve")]
    public async Task<IActionResult> ApproveDraft(Guid draftId, [FromQuery] Guid? lessonId, CancellationToken ct)
    {
        await _lessons.ApproveDraftAsync(draftId, lessonId, ct);
        return NoContent();
    }

    [HttpPost("ai/drafts/{draftId:guid}/reject")]
    public async Task<IActionResult> RejectDraft(Guid draftId, CancellationToken ct)
    {
        await _lessons.RejectDraftAsync(draftId, ct);
        return NoContent();
    }

    // ---------------------------------------------------------------- Lesson Text Extract

    [HttpPost("ai/extract-summary")]
    public async Task<ActionResult<LessonSummaryDto>> ExtractSummary(ExtractTranscriptRequest request, CancellationToken ct)
        => Ok(await _lessons.ExtractAndSummarizeAsync(request, ct));
}
