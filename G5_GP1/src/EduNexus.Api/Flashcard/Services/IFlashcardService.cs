using EduNexus.Api.Flashcard.DTOs;

namespace EduNexus.Api.Flashcard.Services
{
    public interface IFlashcardService
    {
        Task<Guid> CreateGroupAsync(UpsertFlashcardGroupRequest request, CancellationToken ct = default);
        Task<Guid> UpsertCardAsync(UpsertFlashcardRequest request, CancellationToken ct = default);
        Task DeleteGroupAsync(Guid groupId, bool deleteCards, CancellationToken ct = default);
        Task<List<FlashcardGroupDto>> GetLibraryAsync(Guid moduleId, CancellationToken ct = default);
        Task<FlashcardPracticeStatusDto> GetPracticeStatusAsync(Guid moduleId, Guid studentId, CancellationToken ct = default);
        Task MarkAsync(Guid flashcardId, MarkFlashcardRequest request, CancellationToken ct = default);
        Task<AiFlashcardDraftDto> GenerateDraftAsync(GenerateFlashcardRequest request, Guid smeId, CancellationToken ct = default);
        Task UpdateDraftJsonAsync(Guid draftId, string generatedJson, CancellationToken ct = default);
        Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default);
        Task RejectDraftAsync(Guid draftId, CancellationToken ct = default);
    }
}
