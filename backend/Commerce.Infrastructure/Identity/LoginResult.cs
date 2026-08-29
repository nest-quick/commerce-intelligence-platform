using System;
using System.Collections.Generic;
using System.Text;

namespace Commerce.Infrastructure.Identity;

public class LoginResult
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAt { get; set; }
}
