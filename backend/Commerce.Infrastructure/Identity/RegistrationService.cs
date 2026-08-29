using Commerce.Domain.Entities;
using Commerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Commerce.Infrastructure.Identity;

public class RegistrationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public RegistrationService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    public async Task<IdentityResult> RegisterAsync(
        string email,
        string password,
        string merchantName)
    {
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            Name = merchantName,
            CreatedAt = DateTime.UtcNow
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            MerchantId = merchant.Id,
            Merchant = merchant
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return result;
        }

        const string roleName = "Merchant";

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(
                new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName
                });
        }

        await _userManager.AddToRoleAsync(user, roleName);

        return result;
    }
}
