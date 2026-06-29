using EduNexus.Api.Common.DTOs;

namespace EduNexus.Api.Common.Services;

// Student Dashboard — courses, classes, packages đã đăng ký
public interface IDashboardService
{
    Task<DashboardDto> GetStudentDashboardAsync(Guid studentId, CancellationToken ct = default);
}

public class DashboardService : IDashboardService
{
    public Task<DashboardDto> GetStudentDashboardAsync(Guid studentId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: tổng hợp AccessGrant + ClassStudent + PackageSubscription
}
