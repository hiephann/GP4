using EduNexus.Api.Assignment.DTOs;
using EduNexus.Api.Assignment.Repositories;

namespace EduNexus.Api.Assignment.Services;

// FT-05 / FT-08 — Bài tập tự luận: tạo, nộp, chấm, trả kết quả
public interface IAssignmentService
{
    Task<List<AssignmentListItemDto>> GetListAsync(Guid moduleId, CancellationToken ct = default);
    Task<AssignmentDetailDto?> GetDetailAsync(Guid assignmentId, CancellationToken ct = default);
    Task<Guid> UpsertAsync(UpsertAssignmentRequest request, CancellationToken ct = default);

    Task<Guid> SubmitAsync(Guid assignmentId, SubmitAssignmentRequest request, CancellationToken ct = default);
    Task<SubmissionResultDto?> GetResultAsync(Guid submissionId, CancellationToken ct = default);
    Task ConfirmGradeAsync(Guid submissionId, ConfirmGradeRequest request, CancellationToken ct = default);
}

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignments;

    public AssignmentService(IAssignmentRepository assignments) => _assignments = assignments;

    public Task<List<AssignmentListItemDto>> GetListAsync(Guid moduleId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<AssignmentDetailDto?> GetDetailAsync(Guid assignmentId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<Guid> UpsertAsync(UpsertAssignmentRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: tổng tỷ trọng = 100% (AC-05), khóa rubric khi đã có bài nộp

    public Task<Guid> SubmitAsync(Guid assignmentId, SubmitAssignmentRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: chặn nộp sau hạn/lần 2/rỗng, kích AI chấm sơ bộ bất đồng bộ

    public Task<SubmissionResultDto?> GetResultAsync(Guid submissionId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: chỉ trả điểm sau khi GV xác nhận (BR-08)

    public Task ConfirmGradeAsync(Guid submissionId, ConfirmGradeRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: GV xác nhận điểm cuối (BR-10), gửi NTF-10
}
