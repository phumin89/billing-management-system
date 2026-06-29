using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Infrastructure.OwnerCompanyProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagementInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BillingManagementDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOwnerCompanyProfileStore, OwnerCompanyProfileStore>();

        return services;
    }
}
