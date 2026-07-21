namespace EduNexus.Api.Infrastructure.Ai;

/// <summary>Cấu hình GenAI — section "Ai" trong appsettings.json.</summary>
public class AiOptions
{
    public const string SectionName = "Ai";

    public GeminiOptions Gemini { get; set; } = new();
    public YoutubeOptions Youtube { get; set; } = new();

    public class GeminiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public List<string> ApiKeys { get; set; } = new();
        public string Model { get; set; } = "gemini-2.0-flash";

        public IReadOnlyList<string> GetConfiguredKeys()
        {
            var keys = ApiKeys.Where(key => !string.IsNullOrWhiteSpace(key)).ToList();
            if (!string.IsNullOrWhiteSpace(ApiKey)) keys.Insert(0, ApiKey);
            return keys.Distinct(StringComparer.Ordinal).ToList();
        }
    }

    /// <summary>YouTube Data API v3 — dùng khi video không lấy được phụ đề.</summary>
    public class YoutubeOptions
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
