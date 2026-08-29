using Microsoft.AspNetCore.Identity;
using Commerce.Infrastructure.Persistence;

namespace Commerce.Infrastructure.Identity;

public class LoginService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly ApplicationDbContext _applicationDbContext;

    public LoginService(
        UserManager<ApplicationUser> userManager,
        TokenService tokenService,
        RefreshTokenService refreshTokenService,
        ApplicationDbContext applicationDbContext   )
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _applicationDbContext = applicationDbContext;
    }

    public async Task<LoginResult?> LoginAsync(
        string email,
        string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
        {
            return null;
        }

        var accessToken = await _tokenService.CreateTokenAsync(user);

        var refreshToken = _refreshTokenService.GenerateToken();

        var refreshTokenHash = _refreshTokenService.HashToken(refreshToken);

        var expiresAt = _refreshTokenService.GetExpiration();

        var freshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = refreshTokenHash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };

        _applicationDbContext.RefreshTokens.Add(freshTokenEntity);

        await _applicationDbContext.SaveChangesAsync();

        return new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = expiresAt
        };
    }
}

/*
This LoginService logs a user in by checking their email and password.
If the credentials are correct, it creates an access token and a refresh
token, hashes and stores the refresh token in the database with its
expiration date, then returns both tokens to the user.
 */
