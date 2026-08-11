using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.OwnerCompanyProfiles;
using BillingManagement.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagementApplication(this IServiceCollection services)
    {
        services.AddScoped<ICompanyProfileCoverService, CompanyProfileCoverService>();
        services.AddScoped<ICompanyProfileIconService, CompanyProfileIconService>();
        services.AddScoped(typeof(ICommandValidator<>), typeof(AnnotationCommandValidator<>));
        return services;
    }
}
