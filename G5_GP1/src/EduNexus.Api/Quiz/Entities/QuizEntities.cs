namespace EduNexus.Api.Quiz.Entities;

// FT-07 — Bài kiểm tra luyện tập (không tính điểm chính thức - BR-06)
public class QuizItem
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid? CourseId { get; set; }
    public string? Title { get; set; }
    public int QuestionCount { get; set; }
    public string? Difficulty { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
}

public class QuizQuestion                                 // snapshot câu hỏi rút vào quiz
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Guid QuestionId { get; set; }
    public int OrderIndex { get; set; }
}

public class QuizAttempt
{
    public Guid Id { get; set; }
    public Guid QuizId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal? Score { get; set; }                  // chỉ tự đánh giá

    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}

public class QuizAttemptAnswer
{
    public Guid Id { get; set; }
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public bool? IsCorrect { get; set; }
}
