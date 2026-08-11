using BillingManagement.Application.Customers.CreateCustomer;
using BillingManagement.Application.Handlers.Validation;
using Mediator;

namespace BillingManagement.Api;

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddBillingManagementMediator(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.Assemblies = [typeof(CreateCustomerCommand), typeof(CreateCustomerHandler)];
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(CommandValidationBehavior<,>)];
        });

        return services;
    }
}
