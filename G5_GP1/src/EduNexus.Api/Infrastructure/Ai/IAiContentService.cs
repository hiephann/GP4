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
}
