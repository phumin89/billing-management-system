using System.Net;
using System.Net.Http.Json;
using BillingManagement.Client.OwnerCompanyProfiles;
using BillingManagement.Contracts.OwnerCompanyProfiles;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileClientTests
{
    [Fact]
    public async Task Create_preserves_validation_errors_from_bad_request()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new
            {
                status = 400,
                code = "validation_failed",
                errors = new Dictionary<string, string[]>
                {
                    ["CompanyName"] = ["Company name is required."],
                    ["Email"] = ["Email format is invalid."]
                }
            })
        };
        var client = CreateClient(response);

        var result = await client.Create(new CreateOwnerCompanyProfileRequest());

        Assert.False(result.Succeeded);
        Assert.Equal(["CompanyName", "Email"], result.Errors.Keys);
        Assert.Equal(["Company name is required."], result.Errors["CompanyName"]);
        Assert.Equal(["Email format is invalid."], result.Errors["Email"]);
        Assert.Null(result.Message);
    }

    [Fact]
    public async Task Create_preserves_duplicate_profile_message_from_conflict()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = JsonContent.Create(new
            {
                status = 409,
                code = "owner_company_profile.already_exists",
                detail = "Owner company profile already exists."
            })
        };
        var client = CreateClient(response);

        var result = await client.Create(new CreateOwnerCompanyProfileRequest());

        Assert.False(result.Succeeded);
        Assert.Empty(result.Errors);
        Assert.Equal("Company profile already exists. Refresh the page to view it.", result.Message);
    }

    private static OwnerCompanyProfileClient CreateClient(HttpResponseMessage response) =>
        new(new HttpClient(new StubHttpMessageHandler(response))
        {
            BaseAddress = new Uri("http://localhost")
        });

    private sealed class StubHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(response);
    }
}
