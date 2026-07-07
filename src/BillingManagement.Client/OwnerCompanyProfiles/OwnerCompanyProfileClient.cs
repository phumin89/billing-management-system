using System.Net;
using System.Net.Http.Json;
using BillingManagement.Contracts.OwnerCompanyProfiles;

namespace BillingManagement.Client.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileClient(HttpClient httpClient)
{
    public async Task<OwnerCompanyProfileResponse?> Get(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/owner-company-profile", cancellationToken);
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
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/owner-company-profile", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            OwnerCompanyProfileResponse? profile = await response.Content.ReadFromJsonAsync<OwnerCompanyProfileResponse>(cancellationToken);
            return SaveOwnerCompanyProfileResult.Success(profile!);
        }

        ValidationProblemResponse? problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(cancellationToken);

        return SaveOwnerCompanyProfileResult.Failed(problem?.Errors ?? []);
    }

    private sealed class ValidationProblemResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];
    }
}
