using EduNexus.Api.Quiz.DTOs;
using EduNexus.Api.Quiz.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Quiz.Controllers;

// Màn hình: Quiz History, New Quiz, Quiz Taking, Quiz Results, Quiz Review — FT-07
[ApiController]
[Route("api/quizzes")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizzes;

    public QuizController(IQuizService quizzes) => _quizzes = quizzes;

    // Quiz History
    [HttpGet("history")]
    public async Task<ActionResult<List<QuizHistoryItemDto>>> History([FromQuery] Guid studentId, CancellationToken ct)
        => Ok(await _quizzes.GetHistoryAsync(studentId, ct));

    // New Quiz
    [HttpPost]
    public async Task<ActionResult<QuizTakingDto>> Create(CreateQuizRequest request, CancellationToken ct)
        => Ok(await _quizzes.CreateAsync(request, ct));

    // Quiz Taking
    [HttpGet("{quizId:guid}/take")]
    public async Task<ActionResult<QuizTakingDto>> Take(Guid quizId, CancellationToken ct)
    {
        var dto = await _quizzes.GetForTakingAsync(quizId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{quizId:guid}/submit")]
    public async Task<ActionResult<QuizResultDto>> Submit(Guid quizId, SubmitQuizRequest request, CancellationToken ct)
        => Ok(await _quizzes.SubmitAsync(quizId, request, ct));

    // Quiz Results / Quiz Review
    [HttpGet("attempts/{attemptId:guid}/result")]
    public async Task<ActionResult<QuizResultDto>> Result(Guid attemptId, CancellationToken ct)
    {
        var dto = await _quizzes.GetResultAsync(attemptId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
