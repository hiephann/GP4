using EduNexus.Api.Question.DTOs;
using EduNexus.Api.Question.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Question.Controllers;

// Màn hình: Question Bank, Detail, AI Staging, Import — FT-03
[ApiController]
[Route("api/questions")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questions;

    public QuestionController(IQuestionService questions) => _questions = questions;

    // Question Bank (lọc + tìm kiếm)
    [HttpGet]
    public async Task<ActionResult<List<QuestionListItemDto>>> Search([FromQuery] QuestionFilter filter, CancellationToken ct)
        => Ok(await _questions.SearchAsync(filter, ct));

    // Question Detail
    [HttpGet("{questionId:guid}")]
    public async Task<ActionResult<QuestionDetailDto>> GetDetail(Guid questionId, CancellationToken ct)
    {
        var dto = await _questions.GetDetailAsync(questionId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Upsert(UpsertQuestionRequest request, CancellationToken ct)
        => Ok(await _questions.UpsertAsync(request, ct));

    [HttpDelete("{questionId:guid}")]
    public async Task<IActionResult> Delete(Guid questionId, CancellationToken ct)
    {
        await _questions.DeleteAsync(questionId, ct);
        return NoContent();
    }

    // Question Import (Excel)
    [HttpPost("import")]
    public async Task<ActionResult<ImportResultDto>> Import([FromQuery] Guid moduleId, IFormFile file, CancellationToken ct)
        => Ok(await _questions.ImportExcelAsync(moduleId, file, ct));

    // AI Question Staging
    [HttpPost("ai/generate")]
    public async Task<ActionResult<AiQuestionDraftDto>> GenerateDraft(GenerateQuestionRequest request, [FromQuery] Guid smeId, CancellationToken ct)
        => Ok(await _questions.GenerateDraftAsync(request, smeId, ct));

    [HttpPost("ai/drafts/{draftId:guid}/approve")]
    public async Task<IActionResult> ApproveDraft(Guid draftId, CancellationToken ct)
    {
        await _questions.ApproveDraftAsync(draftId, ct);
        return NoContent();
    }
}
