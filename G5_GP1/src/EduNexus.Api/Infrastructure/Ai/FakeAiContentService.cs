using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EduNexus.Api.Infrastructure.Ai;

/// <summary>
/// Fallback khi chưa cấu hình Ai:Gemini:ApiKey — sinh nội dung có cấu trúc ngay tại chỗ
/// để 4 màn Lesson vẫn demo được mà không cần mạng/API key.
/// Mọi nội dung đều gắn nhãn [MOCK] để không nhầm với kết quả AI thật.
/// </summary>
public class FakeAiContentService : IAiContentService
{
    private const int CharsPerToken = 4;   // ước lượng thô, chỉ để AiUsageLogs có số liệu

    public Task<AiResult> GenerateLessonAsync(string outline, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var points = SplitIntoPoints(outline);

        var md = new StringBuilder();
        md.AppendLine($"# {FirstLine(outline)}");
        md.AppendLine();
        md.AppendLine("> [MOCK] Nội dung sinh bởi FakeAiContentService — chưa cấu hình `Ai:Gemini:ApiKey`.");
        md.AppendLine();
        md.AppendLine("## Mục tiêu học tập");
        foreach (var p in points)
            md.AppendLine($"- Hiểu và vận dụng được: {p}");
        md.AppendLine();

        var index = 1;
        foreach (var p in points)
        {
            md.AppendLine($"## {index}. {p}");
            md.AppendLine();
            md.AppendLine($"Phần này trình bày khái niệm **{p}**, giải thích vì sao nó quan trọng trong bài học " +
                          "và cách áp dụng vào tình huống thực tế.");
            md.AppendLine();
            md.AppendLine($"*Ví dụ:* một tình huống minh họa cho {p}.");
            md.AppendLine();
            index++;
        }

        md.AppendLine("## Tóm tắt");
        md.AppendLine($"Bài học đã đi qua {points.Count} nội dung chính. Học viên nên ôn lại các ví dụ trước khi làm bài tập.");

        var text = md.ToString();
        sw.Stop();
        return Task.FromResult(new AiResult(text, text.Length / CharsPerToken, (int)sw.ElapsedMilliseconds, "Mock"));
    }

    public Task<AiResult> SummarizeTranscriptAsync(string transcript, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var sentences = transcript
            .Split(new[] { '.', '!', '?', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 20)
            .ToList();

        var md = new StringBuilder();
        md.AppendLine("# Lesson Summary");
        md.AppendLine();
        md.AppendLine("> [MOCK] Tóm tắt sinh bởi FakeAiContentService — chưa cấu hình `Ai:Gemini:ApiKey`.");
        md.AppendLine();
        md.AppendLine("## Tổng quan");
        md.AppendLine(sentences.Count > 0
            ? Truncate(sentences[0], 300)
            : "Transcript quá ngắn để tóm tắt.");
        md.AppendLine();
        md.AppendLine("## Các ý chính");

        foreach (var s in Sample(sentences, 6))
            md.AppendLine($"- {Truncate(s, 180)}");

        md.AppendLine();
        md.AppendLine($"*Độ dài transcript: {transcript.Length:N0} ký tự.*");

        var text = md.ToString();
        sw.Stop();
        return Task.FromResult(new AiResult(text, text.Length / CharsPerToken, (int)sw.ElapsedMilliseconds, "Mock"));
    }

    public async Task<AiResult> GenerateQuestionsAsync(string sourceText, CancellationToken ct = default)
    {
        await Task.Delay(1000, ct);
        var res = "[{\"content\":\"Câu hỏi giả lập?\",\"explanation\":\"Giải thích\",\"difficulty\":\"Medium\",\"options\":[{\"content\":\"A\",\"isCorrect\":true},{\"content\":\"B\",\"isCorrect\":false}]}]";
        return new AiResult(res, 100, 1000, "Fake/Question");
    }

    public async Task<AiResult> GenerateFlashcardsAsync(string sourceText, CancellationToken ct = default)
    {
        await Task.Delay(1000, ct);
        var res = "[{\"frontText\":\"Mặt trước giả lập\",\"backText\":\"Mặt sau giả lập\"}]";
        return new AiResult(res, 80, 1000, "Fake/Flashcard");
    }

    public Task<AiResult> GradeAssignmentAsync(string gradingPrompt, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var criteria = Regex.Matches(gradingPrompt, @"""criterionId""\s*:\s*""(?<id>[0-9a-fA-F-]{36})""\s*,\s*""maxScore""\s*:\s*(?<max>[0-9.]+)")
            .Select(match => new { Id = match.Groups["id"].Value, Max = decimal.Parse(match.Groups["max"].Value, System.Globalization.CultureInfo.InvariantCulture) })
            .ToList();
        var score = gradingPrompt.Length > 800 ? 0.75m : gradingPrompt.Length > 300 ? 0.60m : 0.40m;
        var result = new
        {
            overallScore = criteria.Sum(criterion => Math.Round(criterion.Max * score, 2)),
            criteria = criteria.Select(criterion => new { criterionId = criterion.Id, score = Math.Round(criterion.Max * score, 2), comment = "[MOCK] Cần cấu hình Gemini API key để có nhận xét AI thực tế." })
        };
        var text = JsonSerializer.Serialize(result);
        sw.Stop();
        return Task.FromResult(new AiResult(text, text.Length / CharsPerToken, (int)sw.ElapsedMilliseconds, "Mock/AssignmentGrading"));
    }

    private static List<string> SplitIntoPoints(string outline)
    {
        var points = outline
            .Split(new[] { '\n', ';', '-', '•' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim(' ', '.', ',', '\r'))
            .Where(s => s.Length > 2)
            .Take(5)
            .ToList();

        return points.Count > 0 ? points : new List<string> { outline.Trim() };
    }

    private static IEnumerable<string> Sample(List<string> items, int count)
    {
        if (items.Count <= count) return items;
        var step = (double)items.Count / count;
        return Enumerable.Range(0, count).Select(i => items[(int)(i * step)]);
    }

    private static string FirstLine(string text)
    {
        var line = text.Split('\n').FirstOrDefault()?.Trim();
        return string.IsNullOrWhiteSpace(line) ? "Bài giảng" : Truncate(line, 120);
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max] + "…";
}
