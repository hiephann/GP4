namespace EduNexus.Api.Common.Entities;

// FT-12 — Tiến độ học tập cá nhân (phần bài giảng)
public class LessonProgress
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Guid StudentId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

// VS 4.3 — Giám sát token AI
public class AiUsageLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TaskType { get; set; } = string.Empty; // GenQuestion | GenFlashcard | Summary | GradeEssay ...
    public long TokensUsed { get; set; }
    public int? DurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
}

// FT-15 / BR-25 — Nhật ký hoạt động hệ thống (không xóa qua UI)
public class ActivityLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Result { get; set; }
    public string? Detail { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Phần 6 SRS — Danh mục thông báo NTF-01..15
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Channel { get; set; } = "InApp";     // InApp | Email | Push
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
