using EduNexus.Api.Common.DTOs;
using EduNexus.Api.Common.Repositories;

namespace EduNexus.Api.Common.Services;

// FT-14 — Xác thực & Hồ sơ người dùng
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken ct = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;

    public AuthService(IUserRepository users) => _users = users;

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: kiểm tra email tồn tại, hash mật khẩu, gửi email xác minh

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: xác thực mật khẩu, tạo phiên đăng nhập

    public Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: verify Google id_token, liên kết tài khoản (BR-26)

    public Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: tạo reset token hiệu lực 1 giờ (BR-22)

    public Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO: đặt lại mật khẩu, đăng xuất các phiên khác (BR-23)

    public Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
        => throw new NotImplementedException(); // TODO
}
