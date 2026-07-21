using EduNexus.Api.Assignment.Repositories;
using EduNexus.Api.Assignment.Services;
using EduNexus.Api.Flashcard.Repositories;
using EduNexus.Api.Flashcard.Services;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Infrastructure.Ai;
using EduNexus.Api.Lesson.Repositories;
using EduNexus.Api.Lesson.Services;
using EduNexus.Api.Question.Repositories;
using EduNexus.Api.Question.Services;
using EduNexus.Web.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
// The Windows Event Log provider can make request handling fail when the app
// is launched without permission to write the .NET Runtime event source.
// Console/Debug logging is sufficient for local development and demo use.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// The Google token is sent as a query value to Google's validation endpoint.
// Keep HttpClient request diagnostics below Information so credentials are never
// written to a local console or CI log.
builder.Logging.AddFilter("System.Net.Http.HttpClient.GoogleTokenValidation", LogLevel.Warning);
// Keep development keys with the project instead of the current Windows
// profile. This prevents stale DPAPI-encrypted keys from breaking Blazor
// circuits when the app is launched by another local process/user.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".data-protection")))
    .SetApplicationName("EduNexus.Web");
builder.Services.AddScoped<IFlashcardRepository, FlashcardRepository>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
builder.Services.AddHttpClient("GoogleTokenValidation", client =>
{
    // A failed external Google request must never leave the login page waiting
    // indefinitely. The browser can retry the sign-in button afterwards.
    client.Timeout = TimeSpan.FromSeconds(10);
});

// EF Core DbContext (SQL Server) — dùng Factory cho Blazor Server để tránh xung đột scope
builder.Services.AddDbContextFactory<EduNexusDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EduNexus.Web.Services.UserSession>();

var authentication = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authentication.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

// Các repository nhận EduNexusDbContext theo scope — lấy từ factory ở trên.
builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<EduNexusDbContext>>().CreateDbContext());

// ----- Feature Lesson (FT-02, FT-06): Blazor -> Service -> Repository, dùng chung với EduNexus.Api -----
builder.Services.AddEduNexusAi(builder.Configuration);
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonService, LessonService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
