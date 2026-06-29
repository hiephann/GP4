using EduNexus.Api.Assignment.Entities;
using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;

namespace EduNexus.Api.Assignment.Repositories;

public interface IAssignmentRepository : IRepository<AssignmentItem>
{
    Task<List<AssignmentItem>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task<AssignmentItem?> GetWithCriteriaAsync(Guid assignmentId, CancellationToken ct = default);
    Task<List<Submission>> GetSubmissionsAsync(Guid assignmentId, CancellationToken ct = default);
}

public class AssignmentRepository : EfRepository<AssignmentItem>, IAssignmentRepository
{
    public AssignmentRepository(EduNexusDbContext db) : base(db) { }

    public Task<List<AssignmentItem>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<AssignmentItem?> GetWithCriteriaAsync(Guid assignmentId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: Include(Criteria)

    public Task<List<Submission>> GetSubmissionsAsync(Guid assignmentId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO
}
