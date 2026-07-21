namespace EduNexus.Api.Infrastructure.Ai;

/// <summary>
/// Kết quả một lần gọi GenAI. TokensUsed/DurationMs dùng để ghi bảng AiUsageLogs (VS 4.3).
/// </summary>
public record AiResult(string Text, long TokensUsed, int DurationMs, string Provider);

/// <summary>
/// Cổng GenAI dùng chung cho các feature (FT-02, FT-06).
/// Cài đặt thật: <see cref="GeminiAiContentService"/>; fallback khi chưa cấu hình key: <see cref="FakeAiContentService"/>.
/// </summary>
public interface IAiContentService
{
    /// <summary>AI Lesson Staging — mở rộng đề cương thành bài giảng markdown (chờ SME duyệt, BR-07).</summary>
    Task<AiResult> GenerateLessonAsync(string outline, CancellationToken ct = default);

    /// <summary>Lesson Text Extract — tóm tắt transcript video thành lesson summary (AC-02f).</summary>
    Task<AiResult> SummarizeTranscriptAsync(string transcript, CancellationToken ct = default);

    /// <summary>AI Question Staging — sinh câu hỏi trắc nghiệm (FT-03).</summary>
    Task<AiResult> GenerateQuestionsAsync(string sourceText, CancellationToken ct = default);

    /// <summary>AI Flashcard Staging — sinh flashcards (FT-04).</summary>
    Task<AiResult> GenerateFlashcardsAsync(string sourceText, CancellationToken ct = default);

    /// <summary>Assignment AI grading — returns a JSON rubric assessment for teacher review (FT-08).</summary>
    Task<AiResult> GradeAssignmentAsync(string gradingPrompt, CancellationToken ct = default);
}
