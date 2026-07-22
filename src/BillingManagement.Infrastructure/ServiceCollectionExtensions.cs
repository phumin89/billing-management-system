using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Infrastructure.CompanyMedia;
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
        services.AddSingleton<ICompanyMediaStore>(_ =>
            new FileSystemCompanyMediaStore(new CompanyMediaStorageOptions
            {
                RootPath = configuration[$"{CompanyMediaStorageOptions.SectionName}:RootPath"]
                    ?? throw new InvalidOperationException("Company media storage root path is required.")
            }));

        return services;
    }
}
