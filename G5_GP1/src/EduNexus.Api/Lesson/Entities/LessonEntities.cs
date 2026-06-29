namespace EduNexus.Api.Lesson.Entities;

// FT-02 — Soạn thảo bài giảng & tài liệu học tập
public class Lesson
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<LessonContent> Contents { get; set; } = new List<LessonContent>();
}

public class LessonContent
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string ContentType { get; set; } = string.Empty; // Markdown | YoutubeVideo | File
    public string? MarkdownText { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? VideoSummary { get; set; }           // tóm tắt AI từ transcript
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
}

// AI Lesson Staging — kết quả AI chờ duyệt (BR-07)
public class AiLessonDraft
{
    public Guid Id { get; set; }
    public Guid? LessonId { get; set; }
    public Guid CreatedById { get; set; }
    public string? SourceText { get; set; }
    public string? GeneratedText { get; set; }
    public string Status { get; set; } = "Pending";     // Pending | Approved | Rejected
    public DateTime CreatedAt { get; set; }
}
