namespace EduNexus.Api.Common.Entities;

// FT-09 — Quản lý lớp học
public class Class
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? TeacherId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MaxStudents { get; set; } = 500;        // LI-07
    public decimal Fee { get; set; }                   // 0 = miễn phí
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; }

    public ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
}

public class ClassMaterial
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? MarkdownText { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ClassStudent
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime JoinedAt { get; set; }
    public string Status { get; set; } = "Active";     // Active | Removed
}
