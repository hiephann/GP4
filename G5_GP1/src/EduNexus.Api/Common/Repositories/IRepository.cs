namespace EduNexus.Api.Common.Repositories;

/// <summary>
/// Repository generic dùng chung cho mọi feature (CRUD cơ bản).
/// Các repository của từng feature kế thừa và bổ sung truy vấn riêng.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
