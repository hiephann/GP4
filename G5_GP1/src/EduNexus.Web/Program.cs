using EduNexus.Api.Flashcard.Repositories;
using EduNexus.Api.Flashcard.Services;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Infrastructure.Ai;
using EduNexus.Api.Lesson.Repositories;
using EduNexus.Api.Lesson.Services;
using EduNexus.Web.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IFlashcardRepository, FlashcardRepository>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core DbContext (SQL Server) — dùng Factory cho Blazor Server để tránh xung đột scope
builder.Services.AddDbContextFactory<EduNexusDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EduNexus.Web.Services.UserSession>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "mock-client-id";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "mock-client-secret";
    });

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

app.Run();
