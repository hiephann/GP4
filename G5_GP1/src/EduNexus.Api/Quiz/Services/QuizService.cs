using EduNexus.Api.Quiz.DTOs;
using EduNexus.Api.Quiz.Repositories;

namespace EduNexus.Api.Quiz.Services;

// FT-07 — Làm bài kiểm tra luyện tập & xem kết quả (không tính điểm chính thức)
public interface IQuizService
{
    Task<List<QuizHistoryItemDto>> GetHistoryAsync(Guid studentId, CancellationToken ct = default);
    Task<QuizTakingDto> CreateAsync(CreateQuizRequest request, CancellationToken ct = default);
    Task<QuizTakingDto?> GetForTakingAsync(Guid quizId, CancellationToken ct = default);
    Task<QuizResultDto> SubmitAsync(Guid quizId, SubmitQuizRequest request, CancellationToken ct = default);
    Task<QuizResultDto?> GetResultAsync(Guid attemptId, CancellationToken ct = default);
}

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizzes;

    public QuizService(IQuizRepository quizzes) => _quizzes = quizzes;

    public Task<List<QuizHistoryItemDto>> GetHistoryAsync(Guid studentId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<QuizTakingDto> CreateAsync(CreateQuizRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: rút ngẫu nhiên câu hỏi; nếu thiếu thì lấy tất cả (NAC-07-a)

    public Task<QuizTakingDto?> GetForTakingAsync(Guid quizId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: không trả đáp án đúng khi đang làm (NAC-07-b)

    public Task<QuizResultDto> SubmitAsync(Guid quizId, SubmitQuizRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: chấm điểm, lưu attempt (BR-06 không vào sổ điểm chính thức)

    public Task<QuizResultDto?> GetResultAsync(Guid attemptId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: kết quả + review từng câu, đáp án đúng, giải thích
}
