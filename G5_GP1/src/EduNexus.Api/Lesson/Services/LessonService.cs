using EduNexus.Api.Common.Entities;
using EduNexus.Api.Infrastructure.Ai;
using EduNexus.Api.Infrastructure.Youtube;
using EduNexus.Api.Lesson.DTOs;
using EduNexus.Api.Lesson.Entities;
using EduNexus.Api.Lesson.Repositories;

namespace EduNexus.Api.Lesson.Services;

// FT-02 / FT-06 — Soạn thảo & học bài giảng
public interface ILessonService
{
    // Lesson Editor
    Task<List<ModuleOptionDto>> GetModuleOptionsAsync(CancellationToken ct = default);
    Task<List<LessonListItemDto>> GetLessonsByModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task<Guid> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct = default);
    Task<List<LessonContentDto>> GetContentsAsync(Guid lessonId, CancellationToken ct = default);
    Task<Guid> UpsertContentAsync(Guid lessonId, UpsertLessonContentRequest request, CancellationToken ct = default);
    Task DeleteContentAsync(Guid contentId, CancellationToken ct = default);

    // Lesson View
    Task<LessonViewDto?> GetForViewAsync(Guid lessonId, CancellationToken ct = default);
    Task MarkCompletedAsync(Guid lessonId, Guid studentId, CancellationToken ct = default);
    Task<bool> IsCompletedAsync(Guid lessonId, Guid studentId, CancellationToken ct = default);
    Task<List<UserOptionDto>> GetUserOptionsAsync(CancellationToken ct = default);

    // AI Lesson Staging
    Task<AiLessonDraftDto> GenerateDraftAsync(GenerateLessonRequest request, Guid smeId, CancellationToken ct = default);
    Task<List<AiLessonDraftDto>> GetDraftsAsync(CancellationToken ct = default);
    Task UpdateDraftTextAsync(Guid draftId, string generatedText, CancellationToken ct = default);
    Task ApproveDraftAsync(Guid draftId, Guid? lessonId = null, CancellationToken ct = default);
    Task RejectDraftAsync(Guid draftId, CancellationToken ct = default);

    // Lesson Text Extract
    Task<LessonSummaryDto> ExtractAndSummarizeAsync(ExtractTranscriptRequest request, CancellationToken ct = default);
    Task SaveSummaryAsync(Guid contentId, string summary, CancellationToken ct = default);
    Task<List<LessonContentDto>> GetVideoContentsAsync(CancellationToken ct = default);
}

public class LessonService : ILessonService
{
    private const int MaxTitleLength = 250;

    // BR-04 — chỉ cho phép tài liệu đính kèm thuộc các định dạng sau
    private static readonly string[] AllowedFileExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".zip" };
    private static readonly string[] AllowedContentTypes = { "Markdown", "YoutubeVideo", "File" };

    private readonly ILessonRepository _lessons;
    private readonly IAiContentService _ai;
    private readonly IYoutubeTranscriptService _youtube;

    public LessonService(ILessonRepository lessons, IAiContentService ai, IYoutubeTranscriptService youtube)
    {
        _lessons = lessons;
        _ai = ai;
        _youtube = youtube;
    }

    // ---------------------------------------------------------------- Lesson Editor

    public async Task<List<ModuleOptionDto>> GetModuleOptionsAsync(CancellationToken ct = default)
    {
        var modules = await _lessons.GetModulesForSelectAsync(ct);
        return modules
            .Select(m => new ModuleOptionDto(m.Id, m.Course is null ? m.Title : $"{m.Course.Title} — {m.Title}"))
            .ToList();
    }

    public async Task<List<LessonListItemDto>> GetLessonsByModuleAsync(Guid moduleId, CancellationToken ct = default)
    {
        var lessons = await _lessons.GetByModuleAsync(moduleId, ct);
        var counts = await _lessons.GetContentCountsAsync(lessons.Select(l => l.Id).ToList(), ct);

        return lessons
            .Select(l => new LessonListItemDto(l.Id, l.Title, l.OrderIndex, counts.GetValueOrDefault(l.Id)))
            .ToList();
    }

    public async Task<Guid> CreateLessonAsync(CreateLessonRequest request, CancellationToken ct = default)
    {
        var title = (request.Title ?? string.Empty).Trim();
        if (title.Length == 0)
            throw new LessonValidationException("Tên bài học không được để trống.");
        if (title.Length > MaxTitleLength)
            throw new LessonValidationException($"Tên bài học tối đa {MaxTitleLength} ký tự.");
        if (request.ModuleId == Guid.Empty)
            throw new LessonValidationException("Chưa chọn module cho bài học.");

        var lesson = new Entities.Lesson
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            Title = title,
            OrderIndex = await _lessons.NextLessonOrderAsync(request.ModuleId, ct),
            CreatedAt = DateTime.UtcNow
        };

        await _lessons.AddAsync(lesson, ct);
        await _lessons.SaveChangesAsync(ct);
        return lesson.Id;
    }

    public async Task<List<LessonContentDto>> GetContentsAsync(Guid lessonId, CancellationToken ct = default)
    {
        var contents = await _lessons.GetContentsAsync(lessonId, ct);
        return contents.Select(ToDto).ToList();
    }

    public async Task<Guid> UpsertContentAsync(Guid lessonId, UpsertLessonContentRequest request, CancellationToken ct = default)
    {
        var lesson = await _lessons.GetByIdAsync(lessonId, ct)
            ?? throw new LessonValidationException("Bài học không tồn tại.");

        var contentType = (request.ContentType ?? string.Empty).Trim();
        if (!AllowedContentTypes.Contains(contentType))
            throw new LessonValidationException($"Loại nội dung không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedContentTypes)}.");

        var content = new LessonContent
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            ContentType = contentType,
            OrderIndex = await _lessons.NextContentOrderAsync(lessonId, ct),
            CreatedAt = DateTime.UtcNow
        };

        switch (contentType)
        {
            case "Markdown":
                if (string.IsNullOrWhiteSpace(request.MarkdownText))
                    throw new LessonValidationException("Nội dung markdown không được để trống.");
                content.MarkdownText = request.MarkdownText.Trim();
                break;

            case "YoutubeVideo":
                // Hệ thống chỉ nhúng video YouTube — link nền tảng khác bị từ chối ngay tại tầng nghiệp vụ.
                if (_youtube.TryParseVideoId(request.YoutubeUrl ?? string.Empty) is null)
                    throw new LessonValidationException("Chỉ chấp nhận đường dẫn video YouTube hợp lệ (youtube.com hoặc youtu.be).");
                content.YoutubeUrl = request.YoutubeUrl!.Trim();
                break;

            case "File":
                var fileName = (request.FileName ?? string.Empty).Trim();
                if (fileName.Length == 0)
                    throw new LessonValidationException("Tên tệp không được để trống.");

                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                if (!AllowedFileExtensions.Contains(ext))
                    throw new LessonValidationException(
                        $"Định dạng tệp không được hỗ trợ (BR-04). Cho phép: {string.Join(", ", AllowedFileExtensions)}.");

                content.FileName = fileName;
                content.FileUrl = string.IsNullOrWhiteSpace(request.FileUrl) ? null : request.FileUrl.Trim();
                break;
        }

        await _lessons.AddContentAsync(content, ct);

        lesson.UpdatedAt = DateTime.UtcNow;
        _lessons.Update(lesson);

        await _lessons.SaveChangesAsync(ct);
        return content.Id;
    }

    public async Task DeleteContentAsync(Guid contentId, CancellationToken ct = default)
    {
        var content = await _lessons.GetContentAsync(contentId, ct)
            ?? throw new LessonValidationException("Nội dung không tồn tại.");

        _lessons.RemoveContent(content);
        await _lessons.SaveChangesAsync(ct);
    }

    // ---------------------------------------------------------------- Lesson View

    public async Task<LessonViewDto?> GetForViewAsync(Guid lessonId, CancellationToken ct = default)
    {
        var lesson = await _lessons.GetWithContentsAsync(lessonId, ct);
        if (lesson is null) return null;

        var contents = lesson.Contents.OrderBy(c => c.OrderIndex).Select(ToDto);
        return new LessonViewDto(lesson.Id, lesson.Title, contents);
    }

    public async Task MarkCompletedAsync(Guid lessonId, Guid studentId, CancellationToken ct = default)
    {
        if (studentId == Guid.Empty)
            throw new LessonValidationException("Chưa chọn học viên.");

        // Bảng LessonProgress có unique index (LessonId, StudentId) → phải upsert, không insert mù.
        var progress = await _lessons.GetProgressAsync(lessonId, studentId, ct);

        if (progress is null)
        {
            await _lessons.AddProgressAsync(new LessonProgress
            {
                Id = Guid.NewGuid(),
                LessonId = lessonId,
                StudentId = studentId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            }, ct);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }

        await _lessons.SaveChangesAsync(ct);
    }

    public async Task<bool> IsCompletedAsync(Guid lessonId, Guid studentId, CancellationToken ct = default)
        => (await _lessons.GetProgressAsync(lessonId, studentId, ct))?.IsCompleted == true;

    public async Task<List<UserOptionDto>> GetUserOptionsAsync(CancellationToken ct = default)
    {
        var users = await _lessons.GetUsersAsync(ct);
        return users.Select(u => new UserOptionDto(u.Id, u.DisplayName)).ToList();
    }

    // ---------------------------------------------------------------- AI Lesson Staging

    public async Task<AiLessonDraftDto> GenerateDraftAsync(GenerateLessonRequest request, Guid smeId, CancellationToken ct = default)
    {
        var outline = (request.Outline ?? string.Empty).Trim();
        if (outline.Length < 10)
            throw new LessonValidationException("Đề cương quá ngắn — hãy nhập ít nhất 10 ký tự để AI có đủ ngữ cảnh.");

        var authorId = smeId != Guid.Empty
            ? smeId
            : await _lessons.GetAnyUserIdAsync(ct)
              ?? throw new LessonValidationException("Chưa có người dùng nào trong hệ thống — hãy chạy script demo data trước.");

        var ai = await _ai.GenerateLessonAsync(outline, ct);

        // BR-07 — nội dung AI luôn ở trạng thái Pending, phải được SME duyệt trước khi vào bài giảng.
        var draft = new AiLessonDraft
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            CreatedById = authorId,
            SourceText = outline,
            GeneratedText = ai.Text,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _lessons.AddDraftAsync(draft, ct);
        await LogAiUsageAsync(authorId, "GenLesson", ai, ct);
        await _lessons.SaveChangesAsync(ct);

        return ToDto(draft);
    }

    public async Task<List<AiLessonDraftDto>> GetDraftsAsync(CancellationToken ct = default)
    {
        var drafts = await _lessons.GetDraftsAsync(ct);
        return drafts.Select(ToDto).ToList();
    }

    public async Task UpdateDraftTextAsync(Guid draftId, string generatedText, CancellationToken ct = default)
    {
        var draft = await GetPendingDraftAsync(draftId, ct);

        if (string.IsNullOrWhiteSpace(generatedText))
            throw new LessonValidationException("Nội dung bản nháp không được để trống.");

        draft.GeneratedText = generatedText.Trim();
        await _lessons.SaveChangesAsync(ct);
    }

    public async Task ApproveDraftAsync(Guid draftId, Guid? lessonId = null, CancellationToken ct = default)
    {
        var draft = await GetPendingDraftAsync(draftId, ct);

        var targetLessonId = lessonId ?? draft.LessonId
            ?? throw new LessonValidationException("Hãy chọn bài học để gắn nội dung đã duyệt.");

        if (await _lessons.GetByIdAsync(targetLessonId, ct) is null)
            throw new LessonValidationException("Bài học không tồn tại.");

        if (string.IsNullOrWhiteSpace(draft.GeneratedText))
            throw new LessonValidationException("Bản nháp rỗng, không thể duyệt.");

        // Duyệt = đổ nội dung AI thành một LessonContent thật của bài học.
        await _lessons.AddContentAsync(new LessonContent
        {
            Id = Guid.NewGuid(),
            LessonId = targetLessonId,
            ContentType = "Markdown",
            MarkdownText = draft.GeneratedText,
            OrderIndex = await _lessons.NextContentOrderAsync(targetLessonId, ct),
            CreatedAt = DateTime.UtcNow
        }, ct);

        draft.LessonId = targetLessonId;
        draft.Status = "Approved";

        await _lessons.SaveChangesAsync(ct);
    }

    public async Task RejectDraftAsync(Guid draftId, CancellationToken ct = default)
    {
        var draft = await GetPendingDraftAsync(draftId, ct);
        draft.Status = "Rejected";
        await _lessons.SaveChangesAsync(ct);
    }

    private async Task<AiLessonDraft> GetPendingDraftAsync(Guid draftId, CancellationToken ct)
    {
        var draft = await _lessons.GetDraftAsync(draftId, ct)
            ?? throw new LessonValidationException("Bản nháp không tồn tại.");

        if (draft.Status != "Pending")
            throw new LessonValidationException($"Bản nháp đã ở trạng thái {draft.Status}, không thể thay đổi.");

        return draft;
    }

    // ---------------------------------------------------------------- Lesson Text Extract

    public async Task<LessonSummaryDto> ExtractAndSummarizeAsync(ExtractTranscriptRequest request, CancellationToken ct = default)
    {
        string transcript;
        string source;

        if (!string.IsNullOrWhiteSpace(request.ManualTranscript))
        {
            transcript = request.ManualTranscript.Trim();
            source = "Nhập thủ công";
        }
        else
        {
            if (_youtube.TryParseVideoId(request.YoutubeUrl ?? string.Empty) is null)
                throw new LessonValidationException("Đường dẫn YouTube không hợp lệ.");

            var result = await _youtube.TryGetTranscriptAsync(request.YoutubeUrl!, ct)
                ?? throw new TranscriptUnavailableException(
                    "Không lấy được phụ đề lẫn thông tin video từ YouTube. Hãy dán transcript thủ công vào ô bên dưới.");

            transcript = result.Text;
            source = result.Source == TranscriptSource.Captions
                ? $"Phụ đề YouTube ({result.LanguageCode}{(result.IsAutoGenerated ? ", tự động" : "")})"
                : "YouTube Data API v3 — tiêu đề & mô tả video";
        }

        if (transcript.Length < 50)
            throw new LessonValidationException("Transcript quá ngắn để tóm tắt (tối thiểu 50 ký tự).");

        var ai = await _ai.SummarizeTranscriptAsync(transcript, ct);

        // Lưu tóm tắt vào đúng content video mà SME đang biên tập (AC-02f).
        if (request.LessonContentId is { } contentId)
        {
            var content = await _lessons.GetContentAsync(contentId, ct)
                ?? throw new LessonValidationException("Nội dung bài học không tồn tại.");

            content.VideoSummary = ai.Text;
        }

        var userId = await _lessons.GetAnyUserIdAsync(ct);
        if (userId is not null)
            await LogAiUsageAsync(userId.Value, "Summary", ai, ct);

        await _lessons.SaveChangesAsync(ct);

        return new LessonSummaryDto(ai.Text, source, transcript.Length, ai.TokensUsed, ai.DurationMs, ai.Provider);
    }

    /// <summary>Lưu bản tóm tắt đã sinh vào một content video — không gọi lại AI (khỏi tốn token lần hai).</summary>
    public async Task SaveSummaryAsync(Guid contentId, string summary, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new LessonValidationException("Chưa có tóm tắt để lưu.");

        var content = await _lessons.GetContentAsync(contentId, ct)
            ?? throw new LessonValidationException("Nội dung bài học không tồn tại.");

        if (content.ContentType != "YoutubeVideo")
            throw new LessonValidationException("Chỉ lưu được tóm tắt vào nội dung loại video YouTube.");

        content.VideoSummary = summary;
        await _lessons.SaveChangesAsync(ct);
    }

    public async Task<List<LessonContentDto>> GetVideoContentsAsync(CancellationToken ct = default)
    {
        var contents = await _lessons.GetVideoContentsAsync(ct);
        return contents.Select(ToDto).ToList();
    }

    // ---------------------------------------------------------------- Helpers

    /// <summary>Ghi AiUsageLogs + cộng dồn Users.AiTokenUsed (VS 4.3 — giám sát token AI).</summary>
    private async Task LogAiUsageAsync(Guid userId, string taskType, AiResult ai, CancellationToken ct)
    {
        await _lessons.AddAiUsageLogAsync(new AiUsageLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskType = taskType,
            TokensUsed = ai.TokensUsed,
            DurationMs = ai.DurationMs,
            CreatedAt = DateTime.UtcNow
        }, ct);

        var user = await _lessons.GetUserAsync(userId, ct);
        if (user is not null)
            user.AiTokenUsed += ai.TokensUsed;
    }

    private static LessonContentDto ToDto(LessonContent c) => new(
        c.Id, c.ContentType, c.MarkdownText, c.YoutubeUrl, c.VideoSummary, c.FileUrl, c.FileName, c.OrderIndex);

    private static AiLessonDraftDto ToDto(AiLessonDraft d) => new(
        d.Id, d.LessonId, d.SourceText, d.GeneratedText, d.Status, d.CreatedAt);
}
