namespace EduNexus.Api.Quiz.DTOs;

// New Quiz — tham số tạo bài kiểm tra luyện tập
public record CreateQuizRequest(Guid StudentId, Guid? CourseId, IEnumerable<Guid> ModuleIds, int QuestionCount, string? Difficulty);

// Quiz Taking
public record QuizQuestionDto(Guid QuestionId, string Content, IEnumerable<QuizOptionDto> Options);
public record QuizOptionDto(Guid OptionId, string Content);
public record QuizTakingDto(Guid QuizId, IEnumerable<QuizQuestionDto> Questions);
public record SubmitQuizRequest(Guid StudentId, IEnumerable<QuizAnswerDto> Answers);
public record QuizAnswerDto(Guid QuestionId, Guid? SelectedOptionId);

// Quiz Results / Review
public record QuizResultDto(Guid AttemptId, decimal Score, int Correct, int Total, IEnumerable<QuizReviewItemDto> Items);
public record QuizReviewItemDto(Guid QuestionId, string Content, Guid? SelectedOptionId, Guid CorrectOptionId, bool IsCorrect, string? Explanation);

// Quiz History
public record QuizHistoryItemDto(Guid AttemptId, Guid QuizId, DateTime? SubmittedAt, decimal? Score);
