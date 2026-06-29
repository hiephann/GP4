using EduNexus.Api.Flashcard.DTOs;
using EduNexus.Api.Flashcard.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Flashcard.Controllers;

// Màn hình: Flashcard Editor, AI Staging, Library, Practice — FT-04, FT-06
[ApiController]
[Route("api/flashcards")]
public class FlashcardController : ControllerBase
{
    private readonly IFlashcardService _flashcards;

    public FlashcardController(IFlashcardService flashcards) => _flashcards = flashcards;

    // Editor
    [HttpPost("groups")]
    public async Task<ActionResult<Guid>> CreateGroup(UpsertFlashcardGroupRequest request, CancellationToken ct)
        => Ok(await _flashcards.CreateGroupAsync(request, ct));

    [HttpPost]
    public async Task<ActionResult<Guid>> UpsertCard(UpsertFlashcardRequest request, CancellationToken ct)
        => Ok(await _flashcards.UpsertCardAsync(request, ct));

    [HttpDelete("groups/{groupId:guid}")]
    public async Task<IActionResult> DeleteGroup(Guid groupId, [FromQuery] bool deleteCards, CancellationToken ct)
    {
        await _flashcards.DeleteGroupAsync(groupId, deleteCards, ct);
        return NoContent();
    }

    // Library
    [HttpGet("library")]
    public async Task<ActionResult<List<FlashcardGroupDto>>> GetLibrary([FromQuery] Guid moduleId, CancellationToken ct)
        => Ok(await _flashcards.GetLibraryAsync(moduleId, ct));

    // Practice
    [HttpGet("practice/status")]
    public async Task<ActionResult<FlashcardPracticeStatusDto>> PracticeStatus([FromQuery] Guid moduleId, [FromQuery] Guid studentId, CancellationToken ct)
        => Ok(await _flashcards.GetPracticeStatusAsync(moduleId, studentId, ct));

    [HttpPost("{flashcardId:guid}/mark")]
    public async Task<IActionResult> Mark(Guid flashcardId, MarkFlashcardRequest request, CancellationToken ct)
    {
        await _flashcards.MarkAsync(flashcardId, request, ct);
        return NoContent();
    }

    // AI Staging
    [HttpPost("ai/generate")]
    public async Task<ActionResult<AiFlashcardDraftDto>> GenerateDraft(GenerateFlashcardRequest request, [FromQuery] Guid smeId, CancellationToken ct)
        => Ok(await _flashcards.GenerateDraftAsync(request, smeId, ct));

    [HttpPost("ai/drafts/{draftId:guid}/approve")]
    public async Task<IActionResult> ApproveDraft(Guid draftId, CancellationToken ct)
    {
        await _flashcards.ApproveDraftAsync(draftId, ct);
        return NoContent();
    }
}
