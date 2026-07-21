using EduNexus.Api.Infrastructure.Youtube;

namespace EduNexus.Api.Infrastructure.Ai;

public static class AiServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký cổng GenAI + dịch vụ lấy transcript YouTube. Dùng chung cho EduNexus.Api và EduNexus.Web.
    /// Chưa cấu hình Ai:Gemini:ApiKey thì tự rơi về FakeAiContentService để demo vẫn chạy.
    /// </summary>
    public static IServiceCollection AddEduNexusAi(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AiOptions.SectionName);
        services.Configure<AiOptions>(section);

        var geminiKeys = section.GetSection("Gemini").Get<AiOptions.GeminiOptions>()?.GetConfiguredKeys() ?? [];

        if (geminiKeys.Count == 0)
        {
            services.AddScoped<IAiContentService, FakeAiContentService>();
        }
        else
        {
            services.AddHttpClient<IAiContentService, GeminiAiContentService>(http =>
            {
                http.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
                http.Timeout = TimeSpan.FromSeconds(60);
            });
        }

        services.AddHttpClient<IYoutubeTranscriptService, YoutubeTranscriptService>(http =>
        {
            http.Timeout = TimeSpan.FromSeconds(30);
            // YouTube trả HTML rút gọn (không kèm captionTracks) nếu thiếu User-Agent của trình duyệt.
            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0 Safari/537.36");
            http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("vi,en;q=0.9");
        });

        return services;
    }
}
