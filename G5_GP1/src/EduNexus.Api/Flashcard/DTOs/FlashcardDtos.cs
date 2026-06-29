namespace EduNexus.Api.Flashcard.DTOs;

// Flashcard Editor
public record UpsertFlashcardGroupRequest(Guid ModuleId, string Name);
public record UpsertFlashcardRequest(Guid? GroupId, Guid ModuleId, string FrontText, string BackText);
public record FlashcardDto(Guid Id, Guid? GroupId, string FrontText, string BackText, int OrderIndex);

// Flashcard Library (Student xem theo nhóm)
public record FlashcardGroupDto(Guid Id, string Name, int CardCount, IEnumerable<FlashcardDto> Cards);

// Flashcard Practice (FT-06)
public record MarkFlashcardRequest(Guid StudentId, bool IsLearned);
public record FlashcardPracticeStatusDto(int Total, int Learned, IEnumerable<Guid> ToReview);

// AI Flashcard Staging
public record GenerateFlashcardRequest(Guid ModuleId, string SourceText);
public record AiFlashcardDraftDto(Guid Id, string? GeneratedJson, string Status);
