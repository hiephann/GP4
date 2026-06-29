namespace EduNexus.Api.Question.Entities;

// FT-03 — Ngân hàng câu hỏi & bài kiểm tra
public class Question
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string Difficulty { get; set; } = "Medium";  // Easy | Medium | Hard
    public string Status { get; set; } = "Active";       // Active | Archived
    public Guid? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}

public class QuestionOption
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }                  // đúng 1 đáp án (AC-03a)
    public int OrderIndex { get; set; }
}

// AI Question Staging — chờ duyệt (BR-07)
public class AiQuestionDraft
{
    public Guid Id { get; set; }
    public Guid? ModuleId { get; set; }
    public Guid CreatedById { get; set; }
    public string? SourceText { get; set; }
    public string? GeneratedJson { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
}
