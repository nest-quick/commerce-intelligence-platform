using Commerce.Api.Contracts.Authentication;
using Commerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegistrationService _registrationService;
    private readonly LoginService _loginService;
    private readonly RefreshTokenService _refreshTokenService;

    public AuthController(RegistrationService registrationService, LoginService loginService, RefreshTokenService refreshTokenService)
    {
        _registrationService = registrationService;
        _loginService = loginService;
        _refreshTokenService = refreshTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _registrationService.RegisterAsync(
            request.Email,
            request.Password,
            request.MerchantName);

        if (!result.Succeeded)
        {
            var errors = new Dictionary<string, string[]>
            {
                ["registration"] = result.Errors
                    .Select(error => error.Description)
                    .ToArray()
            };

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Registration failed.",
                Detail = "One or more registration errors occurred.",
                Instance = HttpContext.Request.Path
            };

            problemDetails.Extensions["code"] =
                "REGISTRATION_FAILED";

            return BadRequest(problemDetails);
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _loginService.LoginAsync(
            request.Email,
            request.Password);

        if (result == null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication failed.",
                Detail = "Invalid email or password.",
                Instance = HttpContext.Request.Path
            };

            problemDetails.Extensions["code"] =
                "INVALID_CREDENTIALS";

            return Unauthorized(problemDetails);
        }

        Response.Cookies.Append(
            "refreshToken",
            result.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiresAt
            });

        return Ok(new LoginResponse
        {
            AccessToken = result.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return InvalidRefreshToken();
        }

        var result = await _refreshTokenService.RefreshAsync(refreshToken);

        if (result == null)
        {
            return InvalidRefreshToken();
        }

        Response.Cookies.Append(
            "refreshToken",
            result.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiresAt
            });

        return Ok(new LoginResponse
        {
            AccessToken = result.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue(
            "refreshToken",
            out var refreshToken))
        {
            await _refreshTokenService.RevokeAsync(refreshToken);
        }

        Response.Cookies.Delete("refreshToken");

        return NoContent();
    }

    private IActionResult InvalidRefreshToken()
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication failed.",
            Detail = "The refresh token is invalid or expired.",
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["code"] =
            "INVALID_REFRESH_TOKEN";

        return Unauthorized(problemDetails);
    }


}

/*

AuthController handles the main authentication requests for the API:

register: creates a new account

login: checks the email/password and returns an access token while storing the refresh token in a cookie

refresh: uses that cookie to generate a new access token and refresh token when the current access token expires.

logout: logs the user out by disabling their refresh token and deleting the refresh token cookie. 

 */
