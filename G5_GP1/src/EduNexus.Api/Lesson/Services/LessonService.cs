using EduNexus.Api.Lesson.DTOs;
using EduNexus.Api.Lesson.Repositories;

namespace EduNexus.Api.Lesson.Services;

// FT-02 / FT-06 — Soạn thảo & học bài giảng
public interface ILessonService
{
    Task<Guid> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct = default);
    Task<Guid> UpsertContentAsync(Guid lessonId, UpsertLessonContentRequest request, CancellationToken ct = default);
    Task<LessonViewDto?> GetForViewAsync(Guid lessonId, CancellationToken ct = default);
    Task MarkCompletedAsync(Guid lessonId, Guid studentId, CancellationToken ct = default);

    // AI Lesson Staging & Lesson Text Extract
    Task<AiLessonDraftDto> GenerateDraftAsync(GenerateLessonRequest request, Guid smeId, CancellationToken ct = default);
    Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default);
    Task<LessonSummaryDto> ExtractAndSummarizeAsync(ExtractTranscriptRequest request, CancellationToken ct = default);
}

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessons;

    public LessonService(ILessonRepository lessons) => _lessons = lessons;

    public Task<Guid> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<Guid> UpsertContentAsync(Guid lessonId, UpsertLessonContentRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: kiểm tra giới hạn file (BR-04), chỉ nhận YouTube

    public Task<LessonViewDto?> GetForViewAsync(Guid lessonId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: kiểm tra quyền truy cập (NAC-06-a)

    public Task MarkCompletedAsync(Guid lessonId, Guid studentId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: ghi LessonProgress

    public Task<AiLessonDraftDto> GenerateDraftAsync(GenerateLessonRequest request, Guid smeId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: gọi AI, lưu draft chờ duyệt (BR-07), trừ token

    public Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<LessonSummaryDto> ExtractAndSummarizeAsync(ExtractTranscriptRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: YouTube Data API -> AI tóm tắt (AC-02f)
}
