using System.Net;
using System.Net.Http.Json;
using BillingManagement.Contracts.OwnerCompanyProfiles;

namespace BillingManagement.Client.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileClient(HttpClient httpClient)
{
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
        var response = await httpClient.PostAsJsonAsync("api/owner-company-profile", request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<OwnerCompanyProfileResponse>(cancellationToken);
            return SaveOwnerCompanyProfileResult.Success(profile!);
        }

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(cancellationToken);

        return SaveOwnerCompanyProfileResult.Failed(problem?.Errors ?? new Dictionary<string, string[]>());
    }

    private sealed class ValidationProblemResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];
    }
}
