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

    public User? CurrentUser { get; private set; }
    public Role? CurrentRole { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public event Action? OnChange;

    public UserSession(ProtectedSessionStorage sessionStorage, IDbContextFactory<EduNexusDbContext> dbFactory)
    {
        _sessionStorage = sessionStorage;
        _dbFactory = dbFactory;
    }

    public async Task InitializeAsync()
    {
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
            // Session read error
        }
    }

    public async Task<bool> LoginWithEmailAsync(string email, string password)
    {
        // Mock password checking for demo purposes (accepts any password if email is correct)
        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            await LoginAsync(user.Id);
            return true;
        }
        return false;
    }

    public async Task<bool> LoginWithGoogleAsync(string googleEmail)
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

    public async Task LoginAsync(Guid userId)
    {
        await LoadUserFromDb(userId);
        await _sessionStorage.SetAsync("UserId", userId);
        OnChange?.Invoke();
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        CurrentRole = null;
        await _sessionStorage.DeleteAsync("UserId");
        OnChange?.Invoke();
    }

    private async Task LoadUserFromDb(Guid userId)
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
}
