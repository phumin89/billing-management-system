using System.Net;
using System.Net.Http.Json;
using BillingManagement.Contracts.OwnerCompanyProfiles;

namespace BillingManagement.Client.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileClient(HttpClient httpClient)
{
    public string GetCoverUrl(string cacheKey) =>
        new Uri(
            httpClient.BaseAddress!,
            $"api/owner-company-profile/cover?v={Uri.EscapeDataString(cacheKey)}").AbsoluteUri;

    public async Task<OwnerCompanyProfileResponse?> Get(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api/owner-company-profile", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<OwnerCompanyProfileResponse>(cancellationToken);
    }

    public async Task<SaveOwnerCompanyProfileResult> Create(
        CreateOwnerCompanyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/owner-company-profile", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return SaveOwnerCompanyProfileResult.Failed(
                new Dictionary<string, string[]>(),
                "Could not save company profile. Check the API connection and try again.");
        }

        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<OwnerCompanyProfileResponse>(cancellationToken);
            return SaveOwnerCompanyProfileResult.Success(profile!);
        }

        if (response.StatusCode is HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(cancellationToken);
            return SaveOwnerCompanyProfileResult.Failed(
                problem?.Errors ?? [],
                null);
        }

        if (response.StatusCode is HttpStatusCode.Conflict)
        {
            return SaveOwnerCompanyProfileResult.Failed(
                new Dictionary<string, string[]>(),
                "Company profile already exists. Refresh the page to view it.");
        }

        return SaveOwnerCompanyProfileResult.Failed(
            new Dictionary<string, string[]>(),
            "Could not save company profile. Try again.");
    }

    public async Task<SaveOwnerCompanyProfileResult> Update(
        UpdateOwnerCompanyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync("api/owner-company-profile", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return SaveOwnerCompanyProfileResult.Failed(
                new Dictionary<string, string[]>(),
                "Could not update company profile. Check the API connection and try again.");
        }

        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<OwnerCompanyProfileResponse>(cancellationToken);
            return SaveOwnerCompanyProfileResult.Success(profile!);
        }

        if (response.StatusCode is HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(cancellationToken);
            return SaveOwnerCompanyProfileResult.Failed(problem?.Errors ?? [], null);
        }

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return SaveOwnerCompanyProfileResult.Failed(
                new Dictionary<string, string[]>(),
                "Company profile no longer exists. Refresh and try again.");
        }

        return SaveOwnerCompanyProfileResult.Failed(
            new Dictionary<string, string[]>(),
            "Could not update company profile. Try again.");
    }

    public async Task<DeleteOwnerCompanyProfileResult> Delete(
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync("api/owner-company-profile", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return DeleteOwnerCompanyProfileResult.Failed(
                "Could not delete company profile. Try again.");
        }

        if (response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound)
        {
            return DeleteOwnerCompanyProfileResult.Success();
        }

        if (response.StatusCode is HttpStatusCode.Conflict)
        {
            return DeleteOwnerCompanyProfileResult.Failed(
                "Company profile is used by quotations or invoices and cannot be deleted.");
        }

        return DeleteOwnerCompanyProfileResult.Failed(
            "Could not delete company profile. Try again.");
    }

    public async Task<CompanyProfileCoverResult> UploadCover(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            using var file = new StreamContent(content);
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            form.Add(file, "file", fileName);

            var response = await httpClient.PutAsync(
                "api/owner-company-profile/cover",
                form,
                cancellationToken);

            return response.IsSuccessStatusCode
                ? CompanyProfileCoverResult.Success()
                : CompanyProfileCoverResult.Failed(
                    "Could not upload the cover image. Try a PNG, JPEG, or WebP file.");
        }
        catch (HttpRequestException)
        {
            return CompanyProfileCoverResult.Failed(
                "Could not upload the cover image. Check the API connection and try again.");
        }
    }

    public async Task<CompanyProfileCoverResult> ResetCover(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync("api/owner-company-profile/cover", cancellationToken);
            return response.IsSuccessStatusCode
                ? CompanyProfileCoverResult.Success()
                : CompanyProfileCoverResult.Failed("Could not reset the cover image. Try again.");
        }
        catch (HttpRequestException)
        {
            return CompanyProfileCoverResult.Failed(
                "Could not reset the cover image. Check the API connection and try again.");
        }
    }

    private sealed class ValidationProblemResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];
    }
}
