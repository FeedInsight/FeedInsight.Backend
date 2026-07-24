using FeedInsight.Domain.Common.Interfaces.Security;
using FeedInsight.Domain.Common.Models;

namespace FeedInsight.Domain.Users;

public class User : Entity
{
    // Null means this is a global system user (Super Admin)
    public Guid? TenantId { get; private set; }
    
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    public bool IsLocked { get; private set; } = false;
    public string? LockReason { get; private set; }


    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();


    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { } // private constructor for ef-core

    public User(string firstName, string lastName, string email, string plainTextPassword, IPasswordHasher hasher, Guid? tenantId = null)
    {
        TenantId = tenantId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = hasher.Hash(plainTextPassword);
    }

    public void LockAccount(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason must be provided when locking an account.");

        IsLocked = true;
        LockReason = reason;

        // Crucial Security Step: Instantly invalidate all their active web sessions!
        RevokeAllRefreshTokens();
    }

    public void UnlockAccount()
    {
        IsLocked = false;
        LockReason = null;
    }

    public bool VerifyPassword(string plainTextPassword, IPasswordHasher hasher)
    {
        return hasher.Verify(plainTextPassword, PasswordHash);
    }

    public void ChangePassword(string newPassword, IPasswordHasher hasher)
    {
        PasswordHash = hasher.Hash(newPassword);
        RevokeAllRefreshTokens();
    }

    public void AddRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
    }

    public void RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Token == token);
        refreshToken?.Revoke();
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens)
        {
            token.Revoke();
        }
    }

    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
        {
            return;
        }

        _userRoles.Add(new UserRole(Id, roleId));
    }

    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur => ur.Role != null && ur.Role.Name == roleName);
    }
}
