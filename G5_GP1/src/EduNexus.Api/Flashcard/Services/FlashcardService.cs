using EduNexus.Api.Flashcard.DTOs;
using EduNexus.Api.Flashcard.Entities;
using EduNexus.Api.Flashcard.Repositories;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Infrastructure.Ai;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EduNexus.Api.Flashcard.Services;

public class FlashcardService : IFlashcardService
{
    private readonly EduNexusDbContext _db; // Inject trực tiếp DB Context để thao tác liên bảng phức tạp
    private readonly IAiContentService _ai;

    public FlashcardService(EduNexusDbContext db, IAiContentService ai)
    {
        _db = db;
        _ai = ai;
    }

    #region Flashcard Editor (SME)

    public async Task<Guid> CreateGroupAsync(UpsertFlashcardGroupRequest request, CancellationToken ct = default)
    {
        var maxOrder = await _db.FlashcardGroups
            .Where(g => g.ModuleId == request.ModuleId)
            .Select(g => (int?)g.OrderIndex)
            .MaxAsync(ct) ?? 0;

        var newGroup = new FlashcardGroup
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            Name = request.Name.Trim(),
            OrderIndex = maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        _db.FlashcardGroups.Add(newGroup);
        await _db.SaveChangesAsync(ct);
        return newGroup.Id;
    }

    public async Task<Guid> UpsertCardAsync(UpsertFlashcardRequest request, CancellationToken ct = default)
    {
        // AC-04b: Mặt trước không được vượt quá 500 ký tự
        if (!string.IsNullOrEmpty(request.FrontText) && request.FrontText.Length > 500)
        {
            throw new ArgumentException("Mặt trước của flashcard (thuật ngữ) không được vượt quá 500 ký tự.");
        }

        // Kiểm tra ràng buộc tối đa 200 thẻ cho mỗi Module
        var totalCardsInModule = await _db.Flashcards.CountAsync(c => c.ModuleId == request.ModuleId, ct);
        if (totalCardsInModule >= 200)
        {
            throw new InvalidOperationException("Mỗi module học tập chỉ được phép chứa tối đa 200 thẻ ghi nhớ.");
        }

        var maxOrder = await _db.Flashcards
            .Where(c => c.ModuleId == request.ModuleId && c.GroupId == request.GroupId)
            .Select(c => (int?)c.OrderIndex)
            .MaxAsync(ct) ?? 0;

        var newCard = new FlashcardItem
        {
            Id = Guid.NewGuid(),
            GroupId = request.GroupId,
            ModuleId = request.ModuleId,
            FrontText = request.FrontText.Trim(),
            BackText = request.BackText.Trim(),
            OrderIndex = maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        _db.Flashcards.Add(newCard);
        await _db.SaveChangesAsync(ct);
        return newCard.Id;
    }

    public async Task DeleteGroupAsync(Guid groupId, bool deleteCards, CancellationToken ct = default)
    {
        var group = await _db.FlashcardGroups.FindAsync(new object[] { groupId }, ct);
        if (group == null) return;

        // NAC-04-a: Xử lý xóa nhóm thẻ và cảnh báo mất tiến độ học tập
        if (deleteCards)
        {
            var cardsInGroup = await _db.Flashcards.Where(c => c.GroupId == groupId).ToListAsync(ct);
            var cardIds = cardsInGroup.Select(c => c.Id).ToList();

            // Xóa toàn bộ tiến độ của học viên liên quan đến các thẻ bị xóa này
            var progressItems = await _db.FlashcardProgress.Where(p => cardIds.Contains(p.FlashcardId)).ToListAsync(ct);
            _db.FlashcardProgress.RemoveRange(progressItems);

            // Xóa thẻ
            _db.Flashcards.RemoveRange(cardsInGroup);
        }
        else
        {
            // Nếu không chọn xóa thẻ, chuyển các thẻ thuộc nhóm này về trạng thái "Chưa phân nhóm" (GroupId = null)
            var cardsInGroup = await _db.Flashcards.Where(c => c.GroupId == groupId).ToListAsync(ct);
            foreach (var card in cardsInGroup)
            {
                card.GroupId = null;
            }
        }

        _db.FlashcardGroups.Remove(group);
        await _db.SaveChangesAsync(ct);
    }

    #endregion

    #region Flashcard Library & Practice (Student)

    public async Task<List<FlashcardGroupDto>> GetLibraryAsync(Guid moduleId, CancellationToken ct = default)
    {
        var groups = await _db.FlashcardGroups
            .Include(g => g.Flashcards)
            .Where(g => g.ModuleId == moduleId)
            .OrderBy(g => g.OrderIndex)
            .ToListAsync(ct);

        return groups.Select(g => new FlashcardGroupDto(
            g.Id,
            g.Name,
            g.Flashcards.Count,
            g.Flashcards.OrderBy(c => c.OrderIndex).Select(c => new FlashcardDto(c.Id, c.GroupId, c.FrontText, c.BackText, c.OrderIndex))
        )).ToList();
    }

    public async Task<FlashcardPracticeStatusDto> GetPracticeStatusAsync(Guid moduleId, Guid studentId, CancellationToken ct = default)
    {
        // Lấy toàn bộ danh sách thẻ của module
        var allCardIds = await _db.Flashcards
            .Where(c => c.ModuleId == moduleId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        // Lấy danh sách các thẻ học viên ĐÃ THUỘC
        var learnedCardIds = await _db.FlashcardProgress
            .Where(p => p.StudentId == studentId && p.IsLearned && allCardIds.Contains(p.FlashcardId))
            .Select(p => p.FlashcardId)
            .ToListAsync(ct);

        // Thuật toán ưu tiên: Lọc ra các thẻ chưa học hoặc chưa thuộc đưa lên hàng đợi trước
        var toReviewIds = allCardIds.Except(learnedCardIds).ToList();

        // Thêm các thẻ cũ vào sau để ôn tập xoay vòng nếu cần
        toReviewIds.AddRange(learnedCardIds);

        return new FlashcardPracticeStatusDto(
            Total: allCardIds.Count,
            Learned: learnedCardIds.Count,
            ToReview: toReviewIds
        );
    }

    public async Task MarkAsync(Guid flashcardId, MarkFlashcardRequest request, CancellationToken ct = default)
    {
        var progress = await _db.FlashcardProgress
            .FirstOrDefaultAsync(p => p.FlashcardId == flashcardId && p.StudentId == request.StudentId, ct);

        if (progress == null)
        {
            progress = new FlashcardProgress
            {
                Id = Guid.NewGuid(),
                FlashcardId = flashcardId,
                StudentId = request.StudentId,
                IsLearned = request.IsLearned,
                LastReviewedAt = DateTime.UtcNow
            };
            _db.FlashcardProgress.Add(progress);
        }
        else
        {
            progress.IsLearned = request.IsLearned;
            progress.LastReviewedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    #endregion

    #region AI Flashcard Staging (SME & GenAI)

    public async Task<AiFlashcardDraftDto> GenerateDraftAsync(GenerateFlashcardRequest request, Guid smeId, CancellationToken ct = default)
    {
        if (request.ModuleId == Guid.Empty || string.IsNullOrWhiteSpace(request.SourceText))
            throw new ArgumentException("Module và nội dung nguồn là bắt buộc để AI sinh flashcard.");
        // BR-07: service returns a real Gemini result when configured, otherwise the explicitly-labelled fallback.
        var ai = await _ai.GenerateFlashcardsAsync(request.SourceText.Trim(), ct);
        using (var generated = JsonDocument.Parse(ai.Text))
        {
            if (generated.RootElement.ValueKind != JsonValueKind.Array || generated.RootElement.GetArrayLength() == 0)
                throw new ArgumentException("AI không trả về mảng flashcard hợp lệ.");
        }

        var draft = new AiFlashcardDraft
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            CreatedById = smeId,
            SourceText = request.SourceText,
            GeneratedJson = ai.Text,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.AiFlashcardDrafts.Add(draft);
        await _db.SaveChangesAsync(ct);

        return new AiFlashcardDraftDto(draft.Id, draft.GeneratedJson, draft.Status);
    }

    public async Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default)
    {
        var draft = await _db.AiFlashcardDrafts.FindAsync(new object[] { draftId }, ct);
        if (draft == null || draft.Status == "Approved") return;

        draft.Status = "Approved";

        // Phân tích cú pháp chuỗi JSON được tạo ra từ AI thành danh sách thực thể để chèn vào bảng Thẻ chính thức
        if (!string.IsNullOrEmpty(draft.GeneratedJson) && draft.ModuleId.HasValue)
        {
            using var doc = JsonDocument.Parse(draft.GeneratedJson);
            var root = doc.RootElement;

            var maxOrder = await _db.Flashcards
                .Where(c => c.ModuleId == draft.ModuleId.Value && c.GroupId == null)
                .Select(c => (int?)c.OrderIndex)
                .MaxAsync(ct) ?? 0;

            foreach (var item in root.EnumerateArray())
            {
                maxOrder++;
                // Accept the old draft shape too, so existing demo records remain reviewable.
                var front = item.TryGetProperty("frontText", out var frontText)
                    ? frontText.GetString() ?? "" : item.GetProperty("front").GetString() ?? "";
                var back = item.TryGetProperty("backText", out var backText)
                    ? backText.GetString() ?? "" : item.GetProperty("back").GetString() ?? "";
                if (string.IsNullOrWhiteSpace(front) || string.IsNullOrWhiteSpace(back) || front.Length > 500)
                    throw new ArgumentException("Dữ liệu AI có flashcard không hợp lệ; hãy chỉnh bản nháp trước khi duyệt.");

                var newCard = new FlashcardItem
                {
                    Id = Guid.NewGuid(),
                    GroupId = null, // Đặt vào trạng thái "Chưa phân nhóm" sau khi duyệt từ AI
                    ModuleId = draft.ModuleId.Value,
                    FrontText = front,
                    BackText = back,
                    OrderIndex = maxOrder,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Flashcards.Add(newCard);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    #endregion
}
