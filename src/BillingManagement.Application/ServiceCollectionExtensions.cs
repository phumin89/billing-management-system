using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagementApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateOwnerCompanyProfileHandler>();
        services.AddScoped<GetOwnerCompanyProfileHandler>();
        services.AddScoped<UpdateOwnerCompanyProfileHandler>();

        return services;
    }
}
