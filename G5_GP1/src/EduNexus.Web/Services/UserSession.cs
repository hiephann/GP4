namespace EduNexus.Web.Services;
using EduNexus.Api.Common.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EduNexus.Api.Infrastructure;

public class UserSession
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IDbContextFactory<EduNexusDbContext> _dbFactory;
    private bool _isInitialized;

    public User? CurrentUser { get; private set; }
    public Role? CurrentRole { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public bool IsInitialized => _isInitialized;

    public event Action? OnChange;

    public UserSession(ProtectedSessionStorage sessionStorage, IDbContextFactory<EduNexusDbContext> dbFactory)
    {
        _sessionStorage = sessionStorage;
        _dbFactory = dbFactory;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        
        try
        {
            var result = await _sessionStorage.GetAsync<Guid>("UserId");
            if (result.Success && result.Value != Guid.Empty)
            {
                await LoadUserFromDb(result.Value);
            }
        }
        catch
        {
            // ProtectedSessionStorage can throw during prerender — ignore safely
        }
        finally
        {
            _isInitialized = true;
        }
    }

    public async Task<bool> LoginWithEmailAsync(string email, string password)
    {
        try
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            
            if (user == null) return false;

            // Demo: chấp nhận password cố định hoặc check mock hash
            // Trong dự án thật: dùng BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)
            var validPasswords = new Dictionary<string, string>
            {
                { "admin@edunexus.vn", "admin123" },
                { "sme.nguyen@edunexus.vn", "sme123" },
                { "teacher.tran@edunexus.vn", "teacher123" },
                { "student.le@edunexus.vn", "student123" },
                { "student.pham@edunexus.vn", "student123" },
                { "student.hoang@edunexus.vn", "student123" },
            };

            // Cho phép đăng nhập nếu password đúng HOẶC nếu password = hash mock (backward compat)
            if (validPasswords.TryGetValue(email, out var expectedPw))
            {
                if (password != expectedPw) return false;
            }
            else
            {
                // Tài khoản mới đăng ký: check password match (demo mode: luôn cho qua)
                // Vì đây là demo project, chấp nhận tất cả password cho tài khoản đã tồn tại
            }

            await LoginAsync(user.Id);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginWithGoogleAsync(string googleEmail)
    {
        try
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == googleEmail);
            if (user != null)
            {
                await LoginAsync(user.Id);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LoginAsync(Guid userId)
    {
        await LoadUserFromDb(userId);
        try
        {
            await _sessionStorage.SetAsync("UserId", userId);
        }
        catch
        {
            // Prerender safety
        }
        OnChange?.Invoke();
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        CurrentRole = null;
        try
        {
            await _sessionStorage.DeleteAsync("UserId");
        }
        catch
        {
            // Prerender safety
        }
        OnChange?.Invoke();
    }

    private async Task LoadUserFromDb(Guid userId)
    {
        try
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            CurrentUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (CurrentUser != null)
            {
                var userRole = await db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId);
                if (userRole != null)
                {
                    CurrentRole = await db.Roles.FirstOrDefaultAsync(r => r.Id == userRole.RoleId);
                }
            }
        }
        catch
        {
            CurrentUser = null;
            CurrentRole = null;
        }
    }
}
