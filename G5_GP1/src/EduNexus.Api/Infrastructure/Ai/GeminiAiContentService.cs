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

    private async Task<AiResult> CallAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var url = $"v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";
        var body = new
        {
            system_instruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = new[] { new { role = "user", parts = new[] { new { text = userPrompt } } } },
            generationConfig = new { temperature = 0.7 }
        };

        var sw = Stopwatch.StartNew();
        using var response = await _http.PostAsJsonAsync(url, body, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        sw.Stop();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini trả về {Status}: {Body}", (int)response.StatusCode, json);
            throw new InvalidOperationException(
                $"Gọi Gemini thất bại ({(int)response.StatusCode}). Kiểm tra lại Ai:Gemini:ApiKey và Ai:Gemini:Model.");
        }

        using var doc = JsonDocument.Parse(json);
        var text = ExtractText(doc.RootElement);
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Gemini không trả về nội dung (có thể bị chặn bởi bộ lọc an toàn).");

        var tokens = doc.RootElement.TryGetProperty("usageMetadata", out var usage)
                     && usage.TryGetProperty("totalTokenCount", out var total)
            ? total.GetInt64()
            : 0;

        return new AiResult(text.Trim(), tokens, (int)sw.ElapsedMilliseconds, $"Gemini/{_options.Model}");
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
