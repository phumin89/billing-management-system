using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Infrastructure;
using BillingManagement.Infrastructure.CompanyMedia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.UnitTests.Deployment;

public sealed class DeploymentConfigurationTests
{
    [Fact]
    public void Api_exposes_liveness_and_database_readiness()
    {
        var program = ReadRepositoryFile("src", "BillingManagement.Api", "Program.cs");

        Assert.Contains("MapHealthChecks(\"/health/live\"", program, StringComparison.Ordinal);
        Assert.Contains("MapHealthChecks(\"/health/ready\"", program, StringComparison.Ordinal);
        Assert.Contains("DatabaseHealthCheck", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Default_configuration_does_not_contain_a_database_password()
    {
        var configuration = ReadRepositoryFile(
            "src", "BillingManagement.Api", "appsettings.json");

        Assert.DoesNotContain("Password=", configuration, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Compose_runs_migrations_before_starting_the_api()
    {
        var compose = ReadRepositoryFile("docker-compose.yml");

        Assert.Contains("condition: service_completed_successfully", compose, StringComparison.Ordinal);
        Assert.Contains("condition: service_healthy", compose, StringComparison.Ordinal);
    }

    [Fact]
    public void Production_client_uses_same_origin_api_proxy()
    {
        var program = ReadRepositoryFile("src", "BillingManagement.Client", "Program.cs");

        Assert.Contains("builder.HostEnvironment.IsDevelopment()", program, StringComparison.Ordinal);
        Assert.Contains("builder.HostEnvironment.BaseAddress", program, StringComparison.Ordinal);
    }

    [Fact]
    public void Infrastructure_registers_database_company_media_as_scoped()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=BillingManagement"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddBillingManagementInfrastructure(configuration);

        var registration = Assert.Single(
            services,
            service => service.ServiceType == typeof(ICompanyMediaStore));
        Assert.Equal(typeof(DatabaseCompanyMediaStore), registration.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }

    private static string ReadRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, .. segments]);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find repository file.");
    }
}
