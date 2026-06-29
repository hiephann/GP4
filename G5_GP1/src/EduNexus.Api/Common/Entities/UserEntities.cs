namespace EduNexus.Api.Common.Entities;

// FT-14 / FT-15 — Xác thực & Hồ sơ người dùng
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }          // null nếu là tài khoản Google (BR-21)
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string AuthProvider { get; set; } = "Local"; // Local | Google
    public string? GoogleSubject { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public long? AiTokenQuota { get; set; }            // hạn mức token/tháng (SME, Teacher)
    public long AiTokenUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // Admin, SME, Teacher, CourseManager, Student
    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class UserRole
{
    public Guid UserId { get; set; }
    public int RoleId { get; set; }
    public User? User { get; set; }
    public Role? Role { get; set; }
}

public class LoginSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }            // hiệu lực 1 giờ (BR-22)
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmailVerificationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
