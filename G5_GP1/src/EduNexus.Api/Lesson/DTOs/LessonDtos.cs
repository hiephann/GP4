namespace EduNexus.Api.Lesson.DTOs;

// --- Dropdown / danh sách ---
public record ModuleOptionDto(Guid Id, string Display);
public record UserOptionDto(Guid Id, string DisplayName);
public record LessonListItemDto(Guid Id, string Title, int OrderIndex, int ContentCount);

// --- Lesson Editor ---
public record CreateLessonRequest(Guid ModuleId, string Title);
public record UpsertLessonContentRequest(string ContentType, string? MarkdownText, string? YoutubeUrl, string? FileUrl, string? FileName);
public record LessonContentDto(Guid Id, string ContentType, string? MarkdownText, string? YoutubeUrl, string? VideoSummary, string? FileUrl, string? FileName, int OrderIndex);

// --- Lesson View ---
public record LessonViewDto(Guid Id, string Title, IEnumerable<LessonContentDto> Contents);

// --- AI Lesson Staging ---
public record GenerateLessonRequest(Guid? LessonId, string Outline);
public record AiLessonDraftDto(Guid Id, Guid? LessonId, string? SourceText, string? GeneratedText, string Status, DateTime CreatedAt);
public record UpdateDraftRequest(string GeneratedText);

// --- Lesson Text Extract (YouTube transcript -> AI summary) ---
/// <param name="ManualTranscript">Dùng khi video không có phụ đề — SME tự dán transcript vào.</param>
/// <param name="LessonContentId">Nếu có, tóm tắt được lưu vào LessonContents.VideoSummary của content này.</param>
public record ExtractTranscriptRequest(string YoutubeUrl, string? ManualTranscript = null, Guid? LessonContentId = null);

public record LessonSummaryDto(
    string Summary,
    string TranscriptSource,     // "YouTube (vi)" | "Nhập thủ công"
    int TranscriptLength,
    long TokensUsed,
    int DurationMs,
    string Provider);
