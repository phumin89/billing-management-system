using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Commands;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagementApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<
            ICommandHandler<CreateOwnerCompanyProfileCommand, CreateOwnerCompanyProfileResult>,
            CreateOwnerCompanyProfileHandler>();
        services.AddScoped<
            ICommandValidator<CreateOwnerCompanyProfileCommand>,
            CreateOwnerCompanyProfileValidator>();
        services.AddScoped<GetOwnerCompanyProfileHandler>();
        services.AddScoped<
            ICommandHandler<UpdateOwnerCompanyProfileCommand, UpdateOwnerCompanyProfileResult>,
            UpdateOwnerCompanyProfileHandler>();
        services.AddScoped<
            ICommandValidator<UpdateOwnerCompanyProfileCommand>,
            UpdateOwnerCompanyProfileValidator>();

        return services;
    }
}
