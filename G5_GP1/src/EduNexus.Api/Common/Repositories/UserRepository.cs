using EduNexus.Api.Common.Entities;
using EduNexus.Api.Infrastructure;

namespace EduNexus.Api.Common.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}

public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(EduNexusDbContext db) : base(db) { }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO
}
