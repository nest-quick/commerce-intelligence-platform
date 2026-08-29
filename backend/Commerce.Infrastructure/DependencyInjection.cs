using Commerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Commerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Commerce.Infrastructure.Stores;


namespace Commerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;

                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<RegistrationService>();
        services.AddScoped<TokenService>();
        services.AddScoped<LoginService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<StoreService>();

        return services;
    }
}
