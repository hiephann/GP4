using EduNexus.Api.Common.DTOs;
using EduNexus.Api.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduNexus.Api.Common.Controllers;

// Màn hình: User Login (+ đăng ký, quên/đặt lại mật khẩu, hồ sơ) — FT-14
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
        => Ok(await _auth.RegisterAsync(request, ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        => Ok(await _auth.LoginAsync(request, ct));

    [HttpPost("google-login")]
    public async Task<ActionResult<AuthResponse>> GoogleLogin(GoogleLoginRequest request, CancellationToken ct)
        => Ok(await _auth.GoogleLoginAsync(request, ct));

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await _auth.ForgotPasswordAsync(request, ct);
        return Accepted();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        await _auth.ResetPasswordAsync(request, ct);
        return NoContent();
    }

    [HttpGet("{userId:guid}/profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(Guid userId, CancellationToken ct)
    {
        var profile = await _auth.GetProfileAsync(userId, ct);
        return profile is null ? NotFound() : Ok(profile);
    }
}
