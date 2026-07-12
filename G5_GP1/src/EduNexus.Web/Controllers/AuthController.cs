using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EduNexus.Api.Infrastructure;
using EduNexus.Api.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IDbContextFactory<EduNexusDbContext> _dbFactory;

    public AuthController(IDbContextFactory<EduNexusDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [HttpGet("google")]
    public IActionResult LoginGoogle()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
            return Redirect("/login?error=google_auth_failed");

        var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
        var googleId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email))
            return Redirect("/login?error=no_email");

        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // Auto-register via Google
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = name ?? email,
                AuthProvider = "Google",
                GoogleSubject = googleId,
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            
            // Assign default Student role
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = 5 });
            await db.SaveChangesAsync();
        }

        // Tạo 1 LoginSession làm token ngắn hạn để đẩy qua Blazor Server
        var token = Guid.NewGuid();
        db.LoginSessions.Add(new LoginSession
        {
            Id = token,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await db.SaveChangesAsync();

        return Redirect($"/auth/callback-processor?token={token}");
    }
}
