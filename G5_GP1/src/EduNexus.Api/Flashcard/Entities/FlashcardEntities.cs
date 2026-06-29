namespace EduNexus.Api.Flashcard.Entities;

// FT-04 — Bộ thẻ ghi nhớ
public class FlashcardGroup
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }                   // BR-19: nhóm thẻ thuộc 1 module
    public string Name { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<FlashcardItem> Flashcards { get; set; } = new List<FlashcardItem>();
}

// Đặt tên FlashcardItem để tránh trùng namespace EduNexus.Api.Flashcard
public class FlashcardItem
{
    public Guid Id { get; set; }
    public Guid? GroupId { get; set; }                   // null = "Chưa phân nhóm"
    public Guid ModuleId { get; set; }
    public string FrontText { get; set; } = string.Empty; // <=500 ký tự (AC-04b)
    public string BackText { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AiFlashcardDraft
{
    public Guid Id { get; set; }
    public Guid? ModuleId { get; set; }
    public Guid CreatedById { get; set; }
    public string? SourceText { get; set; }
    public string? GeneratedJson { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
}

// FT-06 — tiến độ ôn thẻ của học viên
public class FlashcardProgress
{
    public Guid Id { get; set; }
    public Guid FlashcardId { get; set; }
    public Guid StudentId { get; set; }
    public bool IsLearned { get; set; }                  // "Đã thuộc" / "Chưa thuộc"
    public DateTime? LastReviewedAt { get; set; }
}
