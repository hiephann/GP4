namespace EduNexus.Api.Assignment.Entities;

// FT-05 / FT-08 — Bài tập tự luận, rubric, nộp bài & chấm điểm
public class AssignmentItem
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PromptMarkdown { get; set; }          // đề bài (Markdown)
    public DateTime? DueDate { get; set; }
    public int MaxChars { get; set; } = 20000;
    public string Status { get; set; } = "Draft";        // Draft | Published
    public Guid? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<RubricCriterion> Criteria { get; set; } = new List<RubricCriterion>();
}

public class RubricCriterion                              // tổng tỷ trọng = 100% (AC-05a)
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }                  // % > 0 (NAC-05-b)
    public decimal MaxScore { get; set; }
    public int OrderIndex { get; set; }
}

public class Submission
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string? ContentText { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = "Submitted";    // Submitted | AiGraded | Confirmed
    public decimal? AiTotalScore { get; set; }           // điểm AI sơ bộ (chỉ GV xem - BR-08)
    public decimal? FinalScore { get; set; }             // điểm GV xác nhận (BR-10)
    public string? TeacherComment { get; set; }
    public Guid? ConfirmedById { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public ICollection<SubmissionCriterionScore> CriterionScores { get; set; } = new List<SubmissionCriterionScore>();
}

public class SubmissionCriterionScore
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid CriterionId { get; set; }
    public decimal? AiScore { get; set; }
    public decimal? FinalScore { get; set; }
    public string? Comment { get; set; }
}
