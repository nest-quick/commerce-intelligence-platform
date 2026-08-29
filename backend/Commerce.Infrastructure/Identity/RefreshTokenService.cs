using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Commerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Infrastructure.Identity;

public class RefreshTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;
    private readonly TokenService _tokenService;
    public RefreshTokenService(IConfiguration configuration,
        ApplicationDbContext dbContext,
        TokenService tokenService)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }

    public DateTime GetExpiration()
    {
        var expirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays");
        return DateTime.UtcNow.AddDays(expirationDays);
    }
    public async Task<RefreshResult?> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (storedToken == null)
        {
            return null;
        }

        if (storedToken.IsRevoked)
        {
            await RevokeTokenChainAsync(storedToken);
            await _dbContext.SaveChangesAsync();

            return null;
        }

        if (storedToken.IsExpired)
        {
            return null;
        }

        var newRefreshToken = GenerateToken();
        var newRefreshTokenHash = HashToken(newRefreshToken);
        var newExpiration = GetExpiration();

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;

        var newStoredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = newRefreshTokenHash,
            UserId = storedToken.UserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = newExpiration
        };

        _dbContext.RefreshTokens.Add(newStoredToken);

        await _dbContext.SaveChangesAsync();

        var accessToken =
            await _tokenService.CreateTokenAsync(storedToken.User);

        return new RefreshResult
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = newExpiration
        };
    }

    public async Task<bool> RevokeAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (storedToken == null)
        {
            return false;
        }

        if (!storedToken.IsActive)
        {
            return false;
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private async Task RevokeTokenChainAsync(RefreshToken token)
    {
        if (string.IsNullOrEmpty(token.ReplacedByTokenHash))
        {
            return;
        }

        var replacementToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                refreshToken =>
                    refreshToken.TokenHash == token.ReplacedByTokenHash);

        if (replacementToken == null)
        {
            return;
        }

        if (replacementToken.IsActive)
        {
            replacementToken.RevokedAt = DateTime.UtcNow;
        }

        await RevokeTokenChainAsync(replacementToken);
    }
}

/*
 
RefreshTokenService manages refresh tokens:
- creates and hashes tokens
- sets their expiration dates
- replaces old tokens with new ones when refreshing
- revokes tokens when they should no longer be used

 */
