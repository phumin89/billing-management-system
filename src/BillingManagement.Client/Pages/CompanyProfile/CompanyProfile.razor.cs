using BillingManagement.Client.OwnerCompanyProfiles;
using BillingManagement.Contracts.OwnerCompanyProfiles;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.CompanyProfile;

public partial class CompanyProfile
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private OwnerCompanyProfileClient Client { get; set; } = default!;

    private ProfileReviewState reviewState = ProfileReviewState.Empty;
    private bool isEditMode;
    private bool showDeleteSnackbar;
    private bool snackbarClosing;
    private bool isLoading = true;
    private bool isSubmitting;
    private bool isDeleting;
    private string? statusMessage;
    private OwnerCompanyProfileResponse? profile;
    private CreateOwnerCompanyProfileRequest form = new();
    private IReadOnlyDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();

    private readonly CompanyProfileSample sample = new(
        "Acme Operations Co., Ltd.",
        "99 Rama IX Road",
        "12th Floor, Unit 1204",
        "Bangkok",
        "10310",
        "Thailand",
        "0105558123456",
        "+66 2 555 0100",
        "billing@acme.example",
        "https://acme.example",
        "acme-logo.png",
        "DBD registration 0105558123456");

    protected override async Task OnInitializedAsync()
    {
        if (this.ApplyRequestedState())
        {
            this.isLoading = false;
            return;
        }

        this.profile = await this.Client.Get();
        this.reviewState = this.profile is null ? ProfileReviewState.Empty : ProfileReviewState.Existing;
        this.isLoading = false;
    }

    private bool ApplyRequestedState()
    {
        var uri = new Uri(this.Navigation.Uri);
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (!parts[0].Equals("state", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            this.ShowState(parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty);
            return true;
        }

        return false;
    }

    private void ShowState(string state)
    {
        switch (state.ToLowerInvariant())
        {
            case "existing":
                this.ShowExisting();
                break;
            case "form":
                this.ShowCreate();
                break;
            case "delete":
                this.ShowDelete();
                break;
            default:
                this.ShowEmpty();
                break;
        }
    }

    private void ShowEmpty()
    {
        this.reviewState = ProfileReviewState.Empty;
        this.isEditMode = false;
        this.showDeleteSnackbar = false;
        this.profile = null;
    }

    private void ShowExisting()
    {
        this.reviewState = ProfileReviewState.Existing;
        this.isEditMode = false;
        this.showDeleteSnackbar = false;
    }

    private void ShowCreate()
    {
        this.form = new CreateOwnerCompanyProfileRequest();
        this.validationErrors = new Dictionary<string, string[]>();
        this.statusMessage = null;
        this.reviewState = ProfileReviewState.Form;
        this.isEditMode = false;
        this.showDeleteSnackbar = false;
    }

    private void ShowEdit()
    {
        this.form = ToRequest(this.profile);
        this.validationErrors = new Dictionary<string, string[]>();
        this.statusMessage = null;
        this.reviewState = ProfileReviewState.Form;
        this.isEditMode = true;
        this.showDeleteSnackbar = false;
    }

    private void ShowDelete()
    {
        this.reviewState = ProfileReviewState.Existing;
        this.isEditMode = false;
        this.statusMessage = null;
        this.snackbarClosing = false;
        this.showDeleteSnackbar = true;
    }

    private string SnackbarClass => this.snackbarClosing ? "company-snackbar is-closing" : "company-snackbar";

    private OwnerCompanyProfileResponse DisplayProfile =>
        this.profile ?? new OwnerCompanyProfileResponse(
            Guid.Empty,
            this.sample.CompanyName,
            this.sample.AddressLine1,
            this.sample.AddressLine2,
            this.sample.City,
            this.sample.PostalCode,
            this.sample.Country,
            this.sample.TaxId,
            this.sample.Phone,
            this.sample.Email,
            this.sample.Website,
            this.sample.LogoReference,
            this.sample.RegistrationReference);

    private async Task CloseDeleteSnackbar() => await this.DismissSnackbar(this.ShowExisting);

    private async Task ConfirmDelete()
    {
        if (this.isDeleting)
        {
            return;
        }

        this.statusMessage = null;
        this.isDeleting = true;
        try
        {
            var result = await this.Client.Delete();
            if (!result.Succeeded)
            {
                this.statusMessage = result.Message;
            }

            await this.DismissSnackbar(result.Succeeded ? this.ShowEmpty : this.ShowExisting);
        }
        finally
        {
            this.isDeleting = false;
        }
    }

    private async Task SaveProfile()
    {
        if (this.isSubmitting)
        {
            return;
        }

        this.validationErrors = new Dictionary<string, string[]>();
        this.statusMessage = null;
        this.isSubmitting = true;
        var result = this.isEditMode
            ? await this.Client.Update(ToUpdateRequest(this.form))
            : await this.Client.Create(this.form);

        this.isSubmitting = false;
        if (!result.Succeeded)
        {
            this.validationErrors = result.Errors;
            this.statusMessage = result.Message;
            return;
        }

        this.profile = result.Profile;
        this.ShowExisting();
    }

    private async Task DismissSnackbar(Action afterClose)
    {
        if (this.snackbarClosing)
        {
            return;
        }

        this.snackbarClosing = true;
        await Task.Delay(220);
        afterClose();
        this.snackbarClosing = false;
    }

    private string FieldError(string fieldName)
    {
        if (this.validationErrors.TryGetValue(fieldName, out var errors))
        {
            return string.Join(" ", errors);
        }

        return fieldName == nameof(CreateOwnerCompanyProfileRequest.CityProvinceState) &&
            this.validationErrors.TryGetValue("City", out var cityErrors)
                ? string.Join(" ", cityErrors)
                : string.Empty;
    }

    private static string PhoneHref(string? phone) =>
        $"tel:{string.Concat((phone ?? string.Empty).Where(character =>
            char.IsDigit(character) || character is '+' or '*' or '#'))}";

    private static CreateOwnerCompanyProfileRequest ToRequest(OwnerCompanyProfileResponse? profile) =>
        profile is null
            ? new CreateOwnerCompanyProfileRequest()
            : new CreateOwnerCompanyProfileRequest
            {
                CompanyName = profile.CompanyName,
                AddressLine1 = profile.AddressLine1,
                AddressLine2 = profile.AddressLine2,
                CityProvinceState = profile.CityProvinceState,
                PostalCode = profile.PostalCode,
                Country = profile.Country,
                TaxId = profile.TaxId,
                Phone = profile.Phone,
                Email = profile.Email,
                Website = profile.Website,
                LogoReference = profile.LogoReference,
                RegistrationNumber = profile.RegistrationNumber
            };

    private static UpdateOwnerCompanyProfileRequest ToUpdateRequest(CreateOwnerCompanyProfileRequest form) =>
        new()
        {
            CompanyName = form.CompanyName,
            AddressLine1 = form.AddressLine1,
            AddressLine2 = form.AddressLine2,
            CityProvinceState = form.CityProvinceState,
            PostalCode = form.PostalCode,
            Country = form.Country,
            TaxId = form.TaxId,
            Phone = form.Phone,
            Email = form.Email,
            Website = form.Website,
            LogoReference = form.LogoReference,
            RegistrationNumber = form.RegistrationNumber
        };
}
