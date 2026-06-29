namespace EduNexus.Api.Assignment.DTOs;

// Assignment List
public record AssignmentListItemDto(Guid Id, string Title, DateTime? DueDate, string Status);

// Assignment Detail (FT-05) — đề bài + rubric
public record RubricCriterionDto(Guid? Id, string Name, decimal Weight, decimal MaxScore);
public record UpsertAssignmentRequest(Guid ModuleId, string Title, string? PromptMarkdown, DateTime? DueDate, IEnumerable<RubricCriterionDto> Criteria);
public record AssignmentDetailDto(Guid Id, string Title, string? PromptMarkdown, DateTime? DueDate, int MaxChars, string Status, IEnumerable<RubricCriterionDto> Criteria);

// Assignment Submit (FT-08)
public record SubmitAssignmentRequest(Guid StudentId, string ContentText);

// Assignment Result (FT-08)
public record CriterionScoreDto(Guid CriterionId, string CriterionName, decimal? Score, string? Comment);
public record SubmissionResultDto(Guid SubmissionId, string Status, decimal? FinalScore, string? TeacherComment, IEnumerable<CriterionScoreDto> CriterionScores);

// Giảng viên xác nhận điểm (BR-10)
public record ConfirmGradeRequest(Guid TeacherId, decimal FinalScore, string? TeacherComment, IEnumerable<CriterionScoreDto> CriterionScores);
