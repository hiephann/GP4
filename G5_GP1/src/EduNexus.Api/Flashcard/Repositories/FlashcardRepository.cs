using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Flashcard.Entities;
using EduNexus.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Flashcard.Repositories;

public interface IFlashcardRepository : IRepository<FlashcardItem>
{
    Task<List<FlashcardGroup>> GetGroupsByModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task<List<FlashcardItem>> GetByGroupAsync(Guid groupId, CancellationToken ct = default);
}

public class FlashcardRepository : EfRepository<FlashcardItem>, IFlashcardRepository
{
    private readonly EduNexusDbContext _db;
    public FlashcardRepository(EduNexusDbContext db) : base(db) { _db = db; }

    public async Task<List<FlashcardGroup>> GetGroupsByModuleAsync(Guid moduleId, CancellationToken ct = default)
    {
        return await _db.FlashcardGroups
            .Include(g => g.Flashcards)
            .Where(g => g.ModuleId == moduleId)
            .OrderBy(g => g.OrderIndex)
            .ToListAsync(ct);
    }

    public async Task<List<FlashcardItem>> GetByGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        return await _db.Flashcards
            .Where(c => c.GroupId == groupId)
            .OrderBy(c => c.OrderIndex)
            .ToListAsync(ct);
    }
}
