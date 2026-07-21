using EduNexus.Api.Assignment.Entities;
using EduNexus.Api.Common.Repositories;
using EduNexus.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
    public Task<List<AssignmentItem>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default) =>
        Db.Assignments.AsNoTracking().Where(item => item.ModuleId == moduleId).OrderBy(item => item.DueDate).ToListAsync(ct);
    public Task<AssignmentItem?> GetWithCriteriaAsync(Guid assignmentId, CancellationToken ct = default) =>
        Db.Assignments.Include(item => item.Criteria).FirstOrDefaultAsync(item => item.Id == assignmentId, ct);
    public Task<List<Submission>> GetSubmissionsAsync(Guid assignmentId, CancellationToken ct = default) =>
        Db.Submissions.AsNoTracking().Include(item => item.CriterionScores).Where(item => item.AssignmentId == assignmentId).ToListAsync(ct);
}
