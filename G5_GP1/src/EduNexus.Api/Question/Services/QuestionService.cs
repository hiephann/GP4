using EduNexus.Api.Question.DTOs;
using EduNexus.Api.Question.Repositories;
using Microsoft.AspNetCore.Http;

namespace EduNexus.Api.Question.Services;

// FT-03 — Ngân hàng câu hỏi: thủ công, import Excel, AI staging
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

    public QuestionService(IQuestionRepository questions) => _questions = questions;

    public Task<List<QuestionListItemDto>> SearchAsync(QuestionFilter filter, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<QuestionDetailDto?> GetDetailAsync(Guid questionId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<Guid> UpsertAsync(UpsertQuestionRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: >=2 đáp án, đúng 1 (AC-03a)

    public Task DeleteAsync(Guid questionId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO

    public Task<ImportResultDto> ImportExcelAsync(Guid moduleId, IFormFile file, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: xử lý từng dòng độc lập, báo lỗi chi tiết (BR-20)

    public Task<AiQuestionDraftDto> GenerateDraftAsync(GenerateQuestionRequest request, Guid smeId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: AI tạo câu hỏi chờ duyệt (BR-07, BR-09)

    public Task ApproveDraftAsync(Guid draftId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO
}
