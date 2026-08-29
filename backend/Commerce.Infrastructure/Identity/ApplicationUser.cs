using System;
using System.Collections.Generic;
using System.Text;
using Commerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Commerce.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

}
