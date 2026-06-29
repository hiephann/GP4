namespace EduNexus.Api.Common.Entities;

// FT-01 — Quản lý cấu trúc khóa học
public class CourseGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<CourseGroupCourse> CourseGroupCourses { get; set; } = new List<CourseGroupCourse>();
}

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? OwnerSmeId { get; set; }              // SME phụ trách (BR-01)
    public string Status { get; set; } = "Draft";      // Draft | Published | Locked
    public bool IsVisible { get; set; } = true;
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<Module> Modules { get; set; } = new List<Module>();
}

public class CourseGroupCourse
{
    public Guid CourseGroupId { get; set; }
    public Guid CourseId { get; set; }
    public CourseGroup? CourseGroup { get; set; }
    public Course? Course { get; set; }
}

public class Module
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }

    public Course? Course { get; set; }
}
