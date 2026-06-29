namespace EduNexus.Api.Question.DTOs;

// Question Bank (danh sách + lọc)
public record QuestionListItemDto(Guid Id, string Content, string Difficulty, string Status);
public record QuestionFilter(Guid? ModuleId, string? Difficulty, string? Status, string? Keyword);

// Question Detail (FT-03)
public record QuestionOptionDto(Guid? Id, string Content, bool IsCorrect, int OrderIndex);
public record UpsertQuestionRequest(Guid ModuleId, string Content, string? Explanation, string Difficulty, IEnumerable<QuestionOptionDto> Options);
public record QuestionDetailDto(Guid Id, Guid ModuleId, string Content, string? Explanation, string Difficulty, string Status, IEnumerable<QuestionOptionDto> Options);

// Question Import (Excel)
public record ImportRowResult(int RowNumber, bool Success, string? Error);
public record ImportResultDto(int TotalRows, int SuccessCount, int ErrorCount, IEnumerable<ImportRowResult> Errors);

// AI Question Staging
public record GenerateQuestionRequest(Guid ModuleId, string SourceText, int Count);
public record AiQuestionDraftDto(Guid Id, string? GeneratedJson, string Status);
