using EduNexus.Api.Infrastructure;
using EduNexus.Api.Infrastructure.Ai;
using EduNexus.Api.Question.DTOs;
using EduNexus.Api.Question.Entities;
using EduNexus.Api.Question.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json;

namespace EduNexus.Api.Question.Services;

public interface IQuestionService
{
    Task<List<QuestionListItemDto>> SearchAsync(QuestionFilter filter, CancellationToken ct = default);
    Task<QuestionDetailDto?> GetDetailAsync(Guid questionId, CancellationToken ct = default);
    Task<Guid> UpsertAsync(UpsertQuestionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid questionId, CancellationToken ct = default);
    Task<ImportResultDto> ImportExcelAsync(Guid moduleId, IFormFile file, CancellationToken ct = default);
    Task<AiQuestionDraftDto> GenerateDraftAsync(GenerateQuestionRequest request, Guid smeId, CancellationToken ct = default);
    Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default);
}

public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questions;
    private readonly EduNexusDbContext _db;
    private readonly IAiContentService _ai;

    public QuestionService(IQuestionRepository questions, EduNexusDbContext db, IAiContentService ai)
    {
        _questions = questions;
        _db = db;
        _ai = ai;
    }

    public async Task<List<QuestionListItemDto>> SearchAsync(QuestionFilter filter, CancellationToken ct = default)
    {
        var entities = await _questions.SearchAsync(filter, ct);
        return entities.Select(q => new QuestionListItemDto(q.Id, q.Content, q.Difficulty, q.Status)).ToList();
    }

    public async Task<QuestionDetailDto?> GetDetailAsync(Guid questionId, CancellationToken ct = default)
    {
        var q = await _questions.GetWithOptionsAsync(questionId, ct);
        if (q == null) return null;

        var optionsDto = q.Options.OrderBy(o => o.OrderIndex)
            .Select(o => new QuestionOptionDto(o.Id, o.Content, o.IsCorrect, o.OrderIndex));

        return new QuestionDetailDto(q.Id, q.ModuleId, q.Content, q.Explanation, q.Difficulty, q.Status, optionsDto);
    }

    public async Task<Guid> UpsertAsync(UpsertQuestionRequest request, CancellationToken ct = default)
    {
        var optionsList = request.Options.ToList();

        // Business Rule AC-03a validation
        if (optionsList.Count < 2)
            throw new ArgumentException("Cần ít nhất 2 đáp án.");
        if (optionsList.Count(o => o.IsCorrect) != 1)
            throw new ArgumentException("Phải có đúng 1 đáp án đúng.");

        var question = new Entities.Question
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            Content = request.Content.Trim(),
            Explanation = request.Explanation,
            Difficulty = request.Difficulty,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            Options = optionsList.Select(o => new QuestionOption
            {
                Id = Guid.NewGuid(),
                Content = o.Content.Trim(),
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).ToList()
        };

        await _db.Questions.AddAsync(question, ct);
        await _db.SaveChangesAsync(ct);
        return question.Id;
    }

    public async Task DeleteAsync(Guid questionId, CancellationToken ct = default)
    {
        var question = await _db.Questions.FindAsync(new object[] { questionId }, ct);
        if (question != null)
        {
            question.Status = "Archived";
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<ImportResultDto> ImportExcelAsync(Guid moduleId, IFormFile file, CancellationToken ct = default)
    {
        // TODO: Cần cài đặt thư viện EPPlus hoặc ClosedXML để đọc file stream ở đây.
        // Tạm thời trả về Mock result để không văng Exception
        if (moduleId == Guid.Empty) throw new ArgumentException("Chưa chọn module nhận câu hỏi.");
        if (file is null || file.Length == 0) throw new ArgumentException("Vui lòng chọn file Excel có dữ liệu.");
        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Chỉ hỗ trợ file .xlsx theo template import.");

        ExcelPackage.License.SetNonCommercialPersonal("EduNexus Demo");
        var errors = new List<ImportRowResult>();
        var total = 0;
        var success = 0;
        await using var stream = file.OpenReadStream();
        using var package = new ExcelPackage(stream);
        var sheet = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new ArgumentException("File Excel không có worksheet nào.");

        for (var row = 2; row <= (sheet.Dimension?.End.Row ?? 0); row++)
        {
            ct.ThrowIfCancellationRequested();
            var content = sheet.Cells[row, 1].Text.Trim();
            if (content.Length == 0) continue;
            total++;
            try
            {
                var options = Enumerable.Range(2, 4).Select(c => sheet.Cells[row, c].Text.Trim())
                    .Where(text => text.Length > 0).ToList();
                var correct = sheet.Cells[row, 6].Text.Trim().ToUpperInvariant();
                var explanation = sheet.Cells[row, 7].Text.Trim();
                var difficulty = sheet.Cells[row, 8].Text.Trim();
                if (options.Count < 2) throw new ArgumentException("Cần tối thiểu 2 đáp án ở cột B:E.");
                if (correct.Length != 1 || correct[0] < 'A' || correct[0] > 'D')
                    throw new ArgumentException("CorrectOption phải là A, B, C hoặc D.");
                var correctIndex = correct[0] - 'A';
                if (correctIndex >= options.Count) throw new ArgumentException("Đáp án đúng không có nội dung tương ứng.");
                if (difficulty.Length == 0) difficulty = "Medium";
                if (difficulty is not ("Easy" or "Medium" or "Hard"))
                    throw new ArgumentException("Difficulty phải là Easy, Medium hoặc Hard.");

                _db.Questions.Add(new Entities.Question
                {
                    Id = Guid.NewGuid(), ModuleId = moduleId, Content = content,
                    Explanation = explanation.Length == 0 ? null : explanation, Difficulty = difficulty,
                    Status = "Active", CreatedAt = DateTime.UtcNow,
                    Options = options.Select((text, index) => new QuestionOption
                    { Id = Guid.NewGuid(), Content = text, IsCorrect = index == correctIndex, OrderIndex = index + 1 }).ToList()
                });
                success++;
            }
            catch (ArgumentException ex) { errors.Add(new ImportRowResult(row, false, ex.Message)); }
        }
        await _db.SaveChangesAsync(ct);
        return new ImportResultDto(total, success, total - success, errors);
    }

    public async Task<AiQuestionDraftDto> GenerateDraftAsync(GenerateQuestionRequest request, Guid smeId, CancellationToken ct = default)
    {
        if (request.ModuleId == Guid.Empty || string.IsNullOrWhiteSpace(request.SourceText))
            throw new ArgumentException("Module và nội dung nguồn là bắt buộc để AI sinh câu hỏi.");
        var ai = await _ai.GenerateQuestionsAsync(request.SourceText.Trim(), ct);
        ValidateAiQuestionJson(ai.Text);

        var draft = new AiQuestionDraft
        {
            Id = Guid.NewGuid(),
            ModuleId = request.ModuleId,
            CreatedById = smeId,
            SourceText = request.SourceText,
            GeneratedJson = ai.Text,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _db.AiQuestionDrafts.AddAsync(draft, ct);
        await _db.SaveChangesAsync(ct);

        return new AiQuestionDraftDto(draft.Id, draft.GeneratedJson, draft.Status);
    }

    public async Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default)
    {
        var draft = await _db.AiQuestionDrafts.FindAsync(new object[] { draftId }, ct);
        if (draft is null || draft.Status != "Pending") return;
        if (draft.ModuleId is null || string.IsNullOrWhiteSpace(draft.GeneratedJson))
            throw new ArgumentException("Bản nháp AI không có module hoặc nội dung hợp lệ.");

        using var json = JsonDocument.Parse(draft.GeneratedJson);
        foreach (var item in json.RootElement.EnumerateArray())
        {
            var content = item.GetProperty("content").GetString()?.Trim();
            var options = item.GetProperty("options").EnumerateArray().Select((option, index) => new QuestionOption
            {
                Id = Guid.NewGuid(), Content = option.GetProperty("content").GetString()?.Trim() ?? string.Empty,
                IsCorrect = option.GetProperty("isCorrect").GetBoolean(), OrderIndex = index + 1
            }).ToList();
            if (string.IsNullOrWhiteSpace(content) || options.Count < 2 || options.Count(o => o.IsCorrect) != 1)
                throw new ArgumentException("Dữ liệu AI có câu hỏi không hợp lệ; hãy chỉnh bản nháp trước khi duyệt.");

            _db.Questions.Add(new Entities.Question
            {
                Id = Guid.NewGuid(), ModuleId = draft.ModuleId.Value, Content = content,
                Explanation = item.TryGetProperty("explanation", out var explanation) ? explanation.GetString() : null,
                Difficulty = item.TryGetProperty("difficulty", out var difficulty) ? difficulty.GetString() ?? "Medium" : "Medium",
                Status = "Active", CreatedAt = DateTime.UtcNow, Options = options
            });
        }
        draft.Status = "Approved";
        await _db.SaveChangesAsync(ct);
    }
    private static void ValidateAiQuestionJson(string value)
    {
        using var json = JsonDocument.Parse(value);
        if (json.RootElement.ValueKind != JsonValueKind.Array || json.RootElement.GetArrayLength() == 0)
            throw new ArgumentException("AI không trả về mảng câu hỏi hợp lệ.");
    }
}
