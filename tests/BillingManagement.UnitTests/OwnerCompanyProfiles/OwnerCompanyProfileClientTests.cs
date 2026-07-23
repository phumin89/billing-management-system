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

    [Fact]
    public async Task Delete_sends_delete_request_to_company_profile_endpoint()
    {
        HttpMethod? method = null;
        string? requestUri = null;
        var client = CreateClient(request =>
        {
            method = request.Method;
            requestUri = request.RequestUri?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        await client.Delete();

        Assert.Equal(HttpMethod.Delete, method);
        Assert.Equal("http://localhost/api/owner-company-profile", requestUri);
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task Delete_treats_absent_profile_as_success(HttpStatusCode statusCode)
    {
        var client = CreateClient(new HttpResponseMessage(statusCode));

        var result = await client.Delete();

        Assert.True(result.Succeeded);
        Assert.Null(result.Message);
    }

    [Fact]
    public async Task Delete_returns_dependency_message_from_conflict()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.Conflict));

        var result = await client.Delete();

        Assert.False(result.Succeeded);
        Assert.Equal(
            "Company profile is used by quotations or invoices and cannot be deleted.",
            result.Message);
    }

    [Fact]
    public async Task Delete_returns_retry_message_from_unexpected_response()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await client.Delete();

        Assert.False(result.Succeeded);
        Assert.Equal("Could not delete company profile. Try again.", result.Message);
    }

    [Fact]
    public async Task Delete_returns_retry_message_when_api_is_unreachable()
    {
        var client = CreateClient(_ =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("API unavailable.")));

        var result = await client.Delete();

        Assert.False(result.Succeeded);
        Assert.Equal("Could not delete company profile. Try again.", result.Message);
    }

    [Fact]
    public async Task Upload_cover_sends_a_multipart_put_request()
    {
        HttpMethod? method = null;
        string? requestUri = null;
        string? contentType = null;
        string? fileName = null;
        var client = CreateClient(async request =>
        {
            method = request.Method;
            requestUri = request.RequestUri?.ToString();
            var form = Assert.IsType<MultipartFormDataContent>(request.Content);
            var file = Assert.Single(form);
            contentType = file.Headers.ContentType?.MediaType;
            fileName = file.Headers.ContentDisposition?.FileName?.Trim('"');
            await file.LoadIntoBufferAsync();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        var result = await client.UploadCover(new MemoryStream([1, 2, 3]), "cover.png", "image/png");

        Assert.True(result.Succeeded);
        Assert.Equal(HttpMethod.Put, method);
        Assert.Equal("http://localhost/api/owner-company-profile/cover", requestUri);
        Assert.Equal("image/png", contentType);
        Assert.Equal("cover.png", fileName);
    }

    [Fact]
    public async Task Upload_cover_returns_concise_message_for_failed_request()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest));

        var result = await client.UploadCover(new MemoryStream([1]), "cover.png", "image/png");

        Assert.False(result.Succeeded);
        Assert.Equal("Could not upload the cover image. Try a PNG, JPEG, or WebP file.", result.Message);
    }

    [Fact]
    public async Task Reset_cover_sends_delete_request_to_cover_endpoint()
    {
        HttpMethod? method = null;
        string? requestUri = null;
        var client = CreateClient(request =>
        {
            method = request.Method;
            requestUri = request.RequestUri?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        var result = await client.ResetCover();

        Assert.True(result.Succeeded);
        Assert.Equal(HttpMethod.Delete, method);
        Assert.Equal("http://localhost/api/owner-company-profile/cover", requestUri);
    }

    [Fact]
    public void Cover_url_uses_the_configured_api_origin_and_cache_key()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NoContent));

        var url = client.GetCoverUrl("saved-cover");

        Assert.Equal("http://localhost/api/owner-company-profile/cover?v=saved-cover", url);
    }

    private static OwnerCompanyProfileClient CreateClient(HttpResponseMessage response) =>
        CreateClient(_ => Task.FromResult(response));

    private static OwnerCompanyProfileClient CreateClient(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync) =>
        new(new HttpClient(new StubHttpMessageHandler(sendAsync))
        {
            BaseAddress = new Uri("http://localhost")
        });

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            sendAsync(request);
    }
}
