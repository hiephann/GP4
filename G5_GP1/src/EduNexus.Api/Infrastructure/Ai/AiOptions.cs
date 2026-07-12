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
        public string Model { get; set; } = "gemini-2.0-flash";
    }

    /// <summary>YouTube Data API v3 — dùng khi video không lấy được phụ đề.</summary>
    public class YoutubeOptions
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
