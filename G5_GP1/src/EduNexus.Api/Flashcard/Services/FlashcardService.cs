using EduNexus.Api.Flashcard.DTOs;
using EduNexus.Api.Flashcard.Repositories;

namespace EduNexus.Api.Flashcard.Services;

// FT-04 / FT-06 — Bộ thẻ ghi nhớ: soạn, thư viện, luyện tập, AI staging
public interface IFlashcardService
{
    // Editor
    Task<Guid> CreateGroupAsync(UpsertFlashcardGroupRequest request, CancellationToken ct = default);
    Task<Guid> UpsertCardAsync(UpsertFlashcardRequest request, CancellationToken ct = default);
    Task DeleteGroupAsync(Guid groupId, bool deleteCards, CancellationToken ct = default);

    // Library & Practice
    Task<List<FlashcardGroupDto>> GetLibraryAsync(Guid moduleId, CancellationToken ct = default);
    Task<FlashcardPracticeStatusDto> GetPracticeStatusAsync(Guid moduleId, Guid studentId, CancellationToken ct = default);
    Task MarkAsync(Guid flashcardId, MarkFlashcardRequest request, CancellationToken ct = default);

    // AI staging
    Task<AiFlashcardDraftDto> GenerateDraftAsync(GenerateFlashcardRequest request, Guid smeId, CancellationToken ct = default);
    Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default);
}

public class FlashcardService : IFlashcardService
{
    private readonly IFlashcardRepository _flashcards;

    public FlashcardService(IFlashcardRepository flashcards) => _flashcards = flashcards;

    public Task<Guid> CreateGroupAsync(UpsertFlashcardGroupRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<Guid> UpsertCardAsync(UpsertFlashcardRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: <=500 ký tự/mặt (AC-04b), tối đa 200 thẻ/module

    public Task DeleteGroupAsync(Guid groupId, bool deleteCards, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: cảnh báo mất tiến độ (NAC-04-a)

    public Task<List<FlashcardGroupDto>> GetLibraryAsync(Guid moduleId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<FlashcardPracticeStatusDto> GetPracticeStatusAsync(Guid moduleId, Guid studentId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: ưu tiên thẻ chưa thuộc

    public Task MarkAsync(Guid flashcardId, MarkFlashcardRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: lưu FlashcardProgress

    public Task<AiFlashcardDraftDto> GenerateDraftAsync(GenerateFlashcardRequest request, Guid smeId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: AI tạo cặp thuật ngữ-định nghĩa chờ duyệt (BR-07)

    public Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO
}
