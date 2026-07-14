using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Commands;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using BillingManagement.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagementApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped(typeof(ICommandValidator<>), typeof(AnnotationCommandValidator<>));
        services.AddScoped<
            ICommandHandler<CreateOwnerCompanyProfileCommand, CreateOwnerCompanyProfileResult>,
            CreateOwnerCompanyProfileHandler>();
        services.AddScoped<GetOwnerCompanyProfileHandler>();
        services.AddScoped<
            ICommandHandler<UpdateOwnerCompanyProfileCommand, UpdateOwnerCompanyProfileResult>,
            UpdateOwnerCompanyProfileHandler>();
        return services;
    }
}
