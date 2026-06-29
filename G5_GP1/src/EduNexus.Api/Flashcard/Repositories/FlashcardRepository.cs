using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Flashcard.Entities;
using EduNexus.Api.Infrastructure;

namespace EduNexus.Api.Flashcard.Repositories;

public interface IFlashcardRepository : IRepository<FlashcardItem>
{
    Task<List<FlashcardGroup>> GetGroupsByModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task<List<FlashcardItem>> GetByGroupAsync(Guid groupId, CancellationToken ct = default);
}

public class FlashcardRepository : EfRepository<FlashcardItem>, IFlashcardRepository
{
    public FlashcardRepository(EduNexusDbContext db) : base(db) { }

    public Task<List<FlashcardGroup>> GetGroupsByModuleAsync(Guid moduleId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<List<FlashcardItem>> GetByGroupAsync(Guid groupId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO
}
