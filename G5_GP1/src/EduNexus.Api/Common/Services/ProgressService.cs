using EduNexus.Api.Common.DTOs;

namespace EduNexus.Api.Common.Services;

// FT-12 — Theo dõi tiến độ học tập cá nhân
public interface IProgressService
{
    Task<PersonalProgressDto> GetPersonalProgressAsync(Guid studentId, Guid courseId, CancellationToken ct = default);
}

public class ProgressService : IProgressService
{
    public Task<PersonalProgressDto> GetPersonalProgressAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: tính % hoàn thành, xu hướng điểm quiz, thẻ đã thuộc
}
