namespace EduNexus.Api.Lesson.DTOs;

// Lesson Editor
public record CreateLessonRequest(Guid ModuleId, string Title);
public record UpsertLessonContentRequest(string ContentType, string? MarkdownText, string? YoutubeUrl, string? FileUrl, string? FileName);
public record LessonContentDto(Guid Id, string ContentType, string? MarkdownText, string? YoutubeUrl, string? VideoSummary, string? FileUrl, string? FileName, int OrderIndex);

// Lesson View
public record LessonViewDto(Guid Id, string Title, IEnumerable<LessonContentDto> Contents);

// AI Lesson Staging
public record GenerateLessonRequest(Guid? LessonId, string Outline);
public record AiLessonDraftDto(Guid Id, string? GeneratedText, string Status);

// Lesson Text Extract (YouTube transcript -> AI summary)
public record ExtractTranscriptRequest(string YoutubeUrl);
public record LessonSummaryDto(string Summary);
