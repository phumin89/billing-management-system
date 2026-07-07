using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.CompanyProfile;

public partial class CompanyProfile
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private ProfileReviewState reviewState = ProfileReviewState.Empty;
    private bool isEditMode;
    private bool showDeleteSnackbar;
    private bool snackbarClosing;

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

    protected override void OnInitialized() => this.ApplyRequestedState();

    private void ApplyRequestedState()
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
            return;
        }
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
    }

    private void ShowExisting()
    {
        this.reviewState = ProfileReviewState.Existing;
        this.isEditMode = false;
        this.showDeleteSnackbar = false;
    }

    private void ShowCreate()
    {
        this.reviewState = ProfileReviewState.Form;
        this.isEditMode = false;
        this.showDeleteSnackbar = false;
    }

    private void ShowEdit()
    {
        this.reviewState = ProfileReviewState.Form;
        this.isEditMode = true;
        this.showDeleteSnackbar = false;
    }

    private void ShowDelete()
    {
        this.reviewState = ProfileReviewState.Existing;
        this.isEditMode = false;
        this.snackbarClosing = false;
        this.showDeleteSnackbar = true;
    }

    private string SnackbarClass => this.snackbarClosing ? "company-snackbar is-closing" : "company-snackbar";

    private async Task CloseDeleteSnackbar() => await this.DismissSnackbar(this.ShowExisting);

    private async Task ConfirmDelete() => await this.DismissSnackbar(this.ShowEmpty);

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

    private string FormValue(string value) => this.isEditMode ? value : string.Empty;
}
