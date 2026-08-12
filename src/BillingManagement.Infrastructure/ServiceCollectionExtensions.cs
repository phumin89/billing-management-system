using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Infrastructure.BillingDocuments;
using BillingManagement.Infrastructure.CompanyMedia;
using BillingManagement.Infrastructure.Customers;
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
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is required.");
        }

        services.AddDbContext<BillingManagementDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IOwnerCompanyProfileStore, OwnerCompanyProfileStore>();
        services.AddScoped<BillingDocumentStore>();
        services.AddScoped<IBillingDocumentStore>(provider => provider.GetRequiredService<BillingDocumentStore>());
        services.AddScoped<IBillingDocumentQueries>(provider => provider.GetRequiredService<BillingDocumentStore>());
        services.AddScoped<CustomerStore>();
        services.AddScoped<ICustomerStore>(provider => provider.GetRequiredService<CustomerStore>());
        services.AddScoped<ICustomerQueries>(provider => provider.GetRequiredService<CustomerStore>());
        services.AddSingleton<ICompanyMediaStore>(_ =>
            new FileSystemCompanyMediaStore(new CompanyMediaStorageOptions
            {
                RootPath = configuration[$"{CompanyMediaStorageOptions.SectionName}:RootPath"]
                    ?? throw new InvalidOperationException("Company media storage root path is required.")
            }));

        return services;
    }
}
