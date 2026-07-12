using EduNexus.Api.Common.Entities;
using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Lesson.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Lesson.Repositories;

public interface ILessonRepository : IRepository<Entities.Lesson>
{
    // --- Lesson ---
    Task<List<Entities.Lesson>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task<Entities.Lesson?> GetWithContentsAsync(Guid lessonId, CancellationToken ct = default);
    Task<int> NextLessonOrderAsync(Guid moduleId, CancellationToken ct = default);

    // --- Module (đổ dropdown cho Lesson Editor / Lesson View) ---
    Task<List<Module>> GetModulesForSelectAsync(CancellationToken ct = default);

    // --- LessonContent ---
    Task<List<LessonContent>> GetContentsAsync(Guid lessonId, CancellationToken ct = default);
    Task<LessonContent?> GetContentAsync(Guid contentId, CancellationToken ct = default);
    Task AddContentAsync(LessonContent content, CancellationToken ct = default);
    void RemoveContent(LessonContent content);
    Task<int> NextContentOrderAsync(Guid lessonId, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetContentCountsAsync(List<Guid> lessonIds, CancellationToken ct = default);
    Task<List<LessonContent>> GetVideoContentsAsync(CancellationToken ct = default);

    // --- AiLessonDraft (AI Lesson Staging) ---
    Task<List<AiLessonDraft>> GetDraftsAsync(CancellationToken ct = default);
    Task<AiLessonDraft?> GetDraftAsync(Guid draftId, CancellationToken ct = default);
    Task AddDraftAsync(AiLessonDraft draft, CancellationToken ct = default);

    // --- Tiến độ & giám sát token ---
    Task<LessonProgress?> GetProgressAsync(Guid lessonId, Guid studentId, CancellationToken ct = default);
    Task AddProgressAsync(LessonProgress progress, CancellationToken ct = default);
    Task AddAiUsageLogAsync(AiUsageLog log, CancellationToken ct = default);
    Task<User?> GetUserAsync(Guid userId, CancellationToken ct = default);
    Task<Guid?> GetAnyUserIdAsync(CancellationToken ct = default);
    Task<List<User>> GetUsersAsync(CancellationToken ct = default);
}

public class LessonRepository : EfRepository<Entities.Lesson>, ILessonRepository
{
    public LessonRepository(EduNexusDbContext db) : base(db) { }

    public Task<List<Entities.Lesson>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default)
        => Set.Where(l => l.ModuleId == moduleId)
              .OrderBy(l => l.OrderIndex)
              .ToListAsync(ct);

    public Task<Entities.Lesson?> GetWithContentsAsync(Guid lessonId, CancellationToken ct = default)
        => Set.Include(l => l.Contents)
              .FirstOrDefaultAsync(l => l.Id == lessonId, ct);

    public async Task<int> NextLessonOrderAsync(Guid moduleId, CancellationToken ct = default)
        => await Set.Where(l => l.ModuleId == moduleId).CountAsync(ct) + 1;

    public Task<List<Module>> GetModulesForSelectAsync(CancellationToken ct = default)
        => Db.Modules.Include(m => m.Course)   // dropdown hiển thị "Khóa học — Module"
                     .OrderBy(m => m.Course!.Title).ThenBy(m => m.OrderIndex)
                     .ToListAsync(ct);

    public Task<List<LessonContent>> GetContentsAsync(Guid lessonId, CancellationToken ct = default)
        => Db.LessonContents.Where(c => c.LessonId == lessonId)
                            .OrderBy(c => c.OrderIndex)
                            .ToListAsync(ct);

    public Task<LessonContent?> GetContentAsync(Guid contentId, CancellationToken ct = default)
        => Db.LessonContents.FirstOrDefaultAsync(c => c.Id == contentId, ct);

    public async Task AddContentAsync(LessonContent content, CancellationToken ct = default)
        => await Db.LessonContents.AddAsync(content, ct);

    public void RemoveContent(LessonContent content) => Db.LessonContents.Remove(content);

    public async Task<int> NextContentOrderAsync(Guid lessonId, CancellationToken ct = default)
        => await Db.LessonContents.Where(c => c.LessonId == lessonId).CountAsync(ct) + 1;

    public async Task<Dictionary<Guid, int>> GetContentCountsAsync(List<Guid> lessonIds, CancellationToken ct = default)
        => await Db.LessonContents
                   .Where(c => lessonIds.Contains(c.LessonId))
                   .GroupBy(c => c.LessonId)
                   .Select(g => new { g.Key, Count = g.Count() })
                   .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

    public Task<List<LessonContent>> GetVideoContentsAsync(CancellationToken ct = default)
        => Db.LessonContents.Where(c => c.ContentType == "YoutubeVideo")
                            .OrderBy(c => c.CreatedAt)
                            .ToListAsync(ct);

    public Task<List<AiLessonDraft>> GetDraftsAsync(CancellationToken ct = default)
        => Db.AiLessonDrafts.OrderByDescending(d => d.CreatedAt).ToListAsync(ct);

    public Task<AiLessonDraft?> GetDraftAsync(Guid draftId, CancellationToken ct = default)
        => Db.AiLessonDrafts.FirstOrDefaultAsync(d => d.Id == draftId, ct);

    public async Task AddDraftAsync(AiLessonDraft draft, CancellationToken ct = default)
        => await Db.AiLessonDrafts.AddAsync(draft, ct);

    public Task<LessonProgress?> GetProgressAsync(Guid lessonId, Guid studentId, CancellationToken ct = default)
        => Db.LessonProgress.FirstOrDefaultAsync(p => p.LessonId == lessonId && p.StudentId == studentId, ct);

    public async Task AddProgressAsync(LessonProgress progress, CancellationToken ct = default)
        => await Db.LessonProgress.AddAsync(progress, ct);

    public async Task AddAiUsageLogAsync(AiUsageLog log, CancellationToken ct = default)
        => await Db.AiUsageLogs.AddAsync(log, ct);

    public Task<User?> GetUserAsync(Guid userId, CancellationToken ct = default)
        => Db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<Guid?> GetAnyUserIdAsync(CancellationToken ct = default)
        => Db.Users.OrderBy(u => u.CreatedAt).Select(u => (Guid?)u.Id).FirstOrDefaultAsync(ct);

    public Task<List<User>> GetUsersAsync(CancellationToken ct = default)
        => Db.Users.Where(u => u.IsActive).OrderBy(u => u.DisplayName).ToListAsync(ct);
}
