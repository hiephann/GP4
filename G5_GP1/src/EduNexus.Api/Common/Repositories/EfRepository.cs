using EduNexus.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Api.Common.Repositories;

/// <summary>
/// Cài đặt EF Core cho IRepository&lt;T&gt;. CRUD cơ bản hoạt động sẵn;
/// logic nghiệp vụ đặc thù để ở tầng Service.
/// </summary>
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly EduNexusDbContext Db;
    protected readonly DbSet<T> Set;

    public EfRepository(EduNexusDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => await Set.ToListAsync(ct);

    public virtual async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
        => await Set.FindAsync(new[] { id }, ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Db.SaveChangesAsync(ct);
}
