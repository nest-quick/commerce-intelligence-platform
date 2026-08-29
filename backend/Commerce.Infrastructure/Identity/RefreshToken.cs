using System;
using System.Collections.Generic;
using System.Text;

namespace Commerce.Infrastructure.Identity;

public class RefreshToken
{
    public Guid Id { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsRevoked => RevokedAt != null;

    public bool IsActive => !IsExpired && !IsRevoked;
}
