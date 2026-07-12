using EduNexus.Api.Infrastructure;
using EduNexus.Api.Infrastructure.Ai;
using EduNexus.Api.Lesson.Repositories;
using EduNexus.Api.Lesson.Services;
using EduNexus.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core DbContext (SQL Server) — dùng Factory cho Blazor Server để tránh xung đột scope
builder.Services.AddDbContextFactory<EduNexusDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EduNexus.Web.Services.UserSession>();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
