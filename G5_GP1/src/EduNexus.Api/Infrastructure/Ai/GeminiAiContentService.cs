using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EduNexus.Api.Infrastructure.Ai;

/// <summary>
/// Cài đặt GenAI thật bằng Google Gemini (generativelanguage.googleapis.com).
/// </summary>
public class GeminiAiContentService : IAiContentService
{
    private const string LessonSystemPrompt =
        "Bạn là chuyên gia nội dung (SME) của nền tảng học tập EduNexus. " +
        "Từ đề cương người dùng cung cấp, hãy soạn một bài giảng hoàn chỉnh bằng tiếng Việt, định dạng Markdown, " +
        "gồm: tiêu đề bài, mục tiêu học tập, các phần nội dung có heading, ví dụ minh họa và phần tóm tắt cuối bài. " +
        "Chỉ trả về Markdown, không thêm lời dẫn.";

    private const string SummarySystemPrompt =
        "Bạn là trợ lý tóm tắt bài giảng của EduNexus. " +
        "Từ transcript video được cung cấp, hãy viết một lesson summary bằng tiếng Việt, định dạng Markdown, " +
        "gồm: một đoạn tổng quan ngắn, danh sách các ý chính theo thứ tự xuất hiện, và các thuật ngữ quan trọng. " +
        "Chỉ trả về Markdown, không thêm lời dẫn.";

    private const string QuestionSystemPrompt =
        "Bạn là chuyên gia giáo dục của nền tảng EduNexus. " +
        "Từ văn bản nguồn được cung cấp, hãy sinh ra 5 câu hỏi trắc nghiệm bằng tiếng Việt. " +
        "Chỉ trả về MỘT mảng JSON hợp lệ, KHÔNG bọc trong markdown block (```json). " +
        "Cấu trúc mỗi object: { \"content\": \"Nội dung câu hỏi\", \"explanation\": \"Giải thích\", \"difficulty\": \"Medium\", \"options\": [ { \"content\": \"Đáp án A\", \"isCorrect\": true }, ... ] }.";

    private const string FlashcardSystemPrompt =
        "Bạn là chuyên gia giáo dục của nền tảng EduNexus. " +
        "Từ văn bản nguồn được cung cấp, hãy sinh ra 10 flashcard (thẻ ghi nhớ) bằng tiếng Việt. " +
        "Chỉ trả về MỘT mảng JSON hợp lệ, KHÔNG bọc trong markdown block (```json). " +
        "Cấu trúc mỗi object: { \"frontText\": \"Thuật ngữ (mặt trước)\", \"backText\": \"Định nghĩa (mặt sau)\" }.";

    private const string AssignmentGradingSystemPrompt =
        "Bạn là trợ lý chấm bài cho EduNexus. Chấm bài theo đúng rubric được cung cấp, công bằng và ngắn gọn. " +
        "Chỉ trả về MỘT JSON object hợp lệ, không markdown: { \"overallScore\": number, \"criteria\": [ { \"criterionId\": \"guid\", \"score\": number, \"comment\": \"nhận xét tiếng Việt\" } ] }. " +
        "Score của từng criterion nằm trong [0, maxScore]; overallScore là tổng score. Đây là điểm sơ bộ để giáo viên duyệt, không phải điểm cuối.";

    private readonly HttpClient _http;
    private readonly AiOptions.GeminiOptions _options;
    private readonly ILogger<GeminiAiContentService> _logger;

    public GeminiAiContentService(HttpClient http, IOptions<AiOptions> options, ILogger<GeminiAiContentService> logger)
    {
        _http = http;
        _options = options.Value.Gemini;
        _logger = logger;
    }

    public Task<AiResult> GenerateLessonAsync(string outline, CancellationToken ct = default)
        => CallAsync(LessonSystemPrompt, $"Đề cương:\n{outline}", ct);

    public Task<AiResult> SummarizeTranscriptAsync(string transcript, CancellationToken ct = default)
        => CallAsync(SummarySystemPrompt, $"Transcript:\n{transcript}", ct);

    public Task<AiResult> GenerateQuestionsAsync(string sourceText, CancellationToken ct = default)
        => CallAsync(QuestionSystemPrompt, $"Văn bản nguồn:\n{sourceText}", ct);

    public Task<AiResult> GenerateFlashcardsAsync(string sourceText, CancellationToken ct = default)
        => CallAsync(FlashcardSystemPrompt, $"Văn bản nguồn:\n{sourceText}", ct);

    public Task<AiResult> GradeAssignmentAsync(string gradingPrompt, CancellationToken ct = default)
        => CallAsync(AssignmentGradingSystemPrompt, gradingPrompt, ct, "application/json");

    private async Task<AiResult> CallAsync(string systemPrompt, string userPrompt, CancellationToken ct, string? responseMimeType = null)
    {
        var generationConfig = new Dictionary<string, object> { ["temperature"] = 0.7 };
        if (responseMimeType is not null) generationConfig["responseMimeType"] = responseMimeType;
        var body = new
        {
            system_instruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = new[] { new { role = "user", parts = new[] { new { text = userPrompt } } } },
            generationConfig
        };

        var keys = _options.GetConfiguredKeys();
        if (keys.Count == 0) throw new InvalidOperationException("Gemini API key is not configured.");
        var sw = Stopwatch.StartNew();
        HttpResponseMessage? response = null;
        string json = string.Empty;
        for (var index = 0; index < keys.Count; index++)
        {
            response?.Dispose();
            // Keep the key out of the URL: HttpClient diagnostics commonly log URLs.
            // Google accepts x-goog-api-key for the Generative Language API.
            using var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{_options.Model}:generateContent")
            {
                Content = JsonContent.Create(body)
            };
            request.Headers.TryAddWithoutValidation("x-goog-api-key", keys[index]);
            response = await _http.SendAsync(request, ct);
            json = await response.Content.ReadAsStringAsync(ct);
            if (response.IsSuccessStatusCode) break;
            var status = (int)response.StatusCode;
            var shouldTryNextKey = status is 400 or 401 or 403 or 429 or 500 or 503;
            if (!shouldTryNextKey || index == keys.Count - 1) break;

            _logger.LogWarning(
                "Gemini key {KeyIndex} was rejected or temporarily unavailable (HTTP {Status}); trying the next configured key.",
                index + 1,
                status);
        }
        sw.Stop();

        using (response)
        {
            if (response is null || !response.IsSuccessStatusCode)
            {
                var status = response is null ? 0 : (int)response.StatusCode;
                _logger.LogError("Gemini returned {Status}: {Body}", status, json);
                throw new InvalidOperationException($"Gemini request failed ({status}). Check the model and API key configuration.");
            }

            using var doc = JsonDocument.Parse(json);
            var text = ExtractText(doc.RootElement);
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Gemini did not return usable content.");

            var tokens = doc.RootElement.TryGetProperty("usageMetadata", out var usage)
                         && usage.TryGetProperty("totalTokenCount", out var total)
                ? total.GetInt64()
                : 0;

            return new AiResult(text.Trim(), tokens, (int)sw.ElapsedMilliseconds, $"Gemini/{_options.Model}");
        }
    }

    /// <summary>Ghép các part text của candidate đầu tiên.</summary>
    private static string ExtractText(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            return string.Empty;

        if (!candidates[0].TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts))
            return string.Empty;

        return string.Concat(parts.EnumerateArray()
            .Select(p => p.TryGetProperty("text", out var t) ? t.GetString() : null));
    }
}
