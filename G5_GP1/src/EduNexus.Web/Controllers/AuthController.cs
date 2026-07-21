using System.Security.Claims;
using System.Text.Json;
using EduNexus.Api.Common.Entities;
using EduNexus.Api.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduNexus.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IDbContextFactory<EduNexusDbContext> _dbFactory;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IDbContextFactory<EduNexusDbContext> dbFactory, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _dbFactory = dbFactory;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    // Traditional server-side OAuth remains available when a client secret is configured.
    [HttpGet("google")]
    public IActionResult LoginGoogle()
    {
        if (string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"]))
            return Redirect("/login?error=google_not_configured");

        // The visible sign-in button uses Google Identity Services and only
        // needs a Web Client ID. Do not send a user to a dead end when an old
        // bookmark or cached page still calls this legacy server-side route.
        if (string.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientSecret"]))
            return Redirect("/login");

        var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleCallback), "Auth") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded) return Redirect("/login?error=google_auth_failed");

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var name = result.Principal.FindFirstValue(ClaimTypes.Name);
        var subject = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject))
            return Redirect("/login?error=no_email");

        return await CompleteGoogleLoginAsync(email, name, subject);
    }

    // Google Identity Services posts an ID token here. This flow needs only a Web Client ID,
    // while the server verifies the token audience and Google-confirmed email before creating a user.
    [HttpPost("google-token")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> GoogleToken([FromForm] string credential)
    {
        var clientId = _configuration["Authentication:Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return Redirect("/login?error=google_not_configured");
        if (string.IsNullOrWhiteSpace(credential))
            return Redirect("/login?error=google_auth_failed");

        try
        {
            var client = _httpClientFactory.CreateClient("GoogleTokenValidation");
            using var response = await client.GetAsync("https://oauth2.googleapis.com/tokeninfo?id_token=" + Uri.EscapeDataString(credential));
            if (!response.IsSuccessStatusCode) return Redirect("/login?error=google_auth_failed");

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var claims = document.RootElement;
            var audience = GetClaim(claims, "aud");
            var issuer = GetClaim(claims, "iss");
            var email = GetClaim(claims, "email");
            var name = GetClaim(claims, "name");
            var subject = GetClaim(claims, "sub");
            var emailVerified = string.Equals(GetClaim(claims, "email_verified"), "true", StringComparison.OrdinalIgnoreCase);

            var trustedIssuer = issuer is "https://accounts.google.com" or "accounts.google.com";
            if (!trustedIssuer || !string.Equals(audience, clientId, StringComparison.Ordinal) || !emailVerified ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(subject))
                return Redirect("/login?error=google_auth_failed");

            return await CompleteGoogleLoginAsync(email, name, subject);
        }
        catch (HttpRequestException)
        {
            return Redirect("/login?error=google_auth_failed");
        }
        catch (TaskCanceledException)
        {
            return Redirect("/login?error=google_auth_failed");
        }
        catch (JsonException)
        {
            return Redirect("/login?error=google_auth_failed");
        }
    }

    private async Task<IActionResult> CompleteGoogleLoginAsync(string email, string? name, string googleSubject)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        var isNewUser = user is null;

        if (isNewUser)
        {
            var studentRole = await db.Roles.SingleOrDefaultAsync(r => r.Name == "Student");
            if (studentRole is null) return Redirect("/login?error=google_auth_failed");

            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = string.IsNullOrWhiteSpace(name) ? email : name,
                AuthProvider = "Google",
                GoogleSubject = googleSubject,
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(user);
            // Save the principal row before creating any dependent records.
            // The existing database schema has FK constraints but no EF navigation
            // properties on UserRole/LoginSession to establish insert ordering.
            await db.SaveChangesAsync();
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = studentRole.Id });
        }
        else
        {
            var existingUser = user!;
            if (!existingUser.IsActive) return Redirect("/login?error=google_auth_failed");
            existingUser.GoogleSubject ??= googleSubject;
            existingUser.AuthProvider = "Google";
            existingUser.IsEmailVerified = true;
            existingUser.UpdatedAt = DateTime.UtcNow;
        }

        var resolvedUser = user ?? throw new InvalidOperationException("Google user could not be resolved.");
        if (!isNewUser && !await db.UserRoles.AnyAsync(role => role.UserId == resolvedUser.Id))
        {
            var studentRole = await db.Roles.SingleOrDefaultAsync(role => role.Name == "Student")
                ?? throw new InvalidOperationException("Student role is not configured.");
            db.UserRoles.Add(new UserRole { UserId = resolvedUser.Id, RoleId = studentRole.Id });
        }

        var token = Guid.NewGuid();
        db.LoginSessions.Add(new LoginSession
        {
            Id = token,
            UserId = resolvedUser.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await db.SaveChangesAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect($"/auth/callback-processor?token={token}");
    }

    private static string? GetClaim(JsonElement claims, string name) =>
        claims.TryGetProperty(name, out var value) ? value.ToString() : null;
}
