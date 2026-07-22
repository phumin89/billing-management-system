using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Commands;
using BillingManagement.Application.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.DeleteOwnerCompanyProfile;
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
        services.AddScoped<ICompanyProfileCoverService, CompanyProfileCoverService>();
        services.AddScoped(typeof(ICommandValidator<>), typeof(AnnotationCommandValidator<>));
        services.AddScoped<
            ICommandHandler<CreateOwnerCompanyProfileCommand, OwnerCompanyProfileRecord>,
            CreateOwnerCompanyProfileHandler>();
        services.AddScoped<
            ICommandHandler<DeleteOwnerCompanyProfileCommand, bool>,
            DeleteOwnerCompanyProfileHandler>();
        services.AddScoped<GetOwnerCompanyProfileHandler>();
        services.AddScoped<
            ICommandHandler<UpdateOwnerCompanyProfileCommand, OwnerCompanyProfileRecord>,
            UpdateOwnerCompanyProfileHandler>();
        return services;
    }
}
