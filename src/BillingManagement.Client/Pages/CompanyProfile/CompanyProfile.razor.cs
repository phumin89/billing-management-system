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

    protected override void OnInitialized() => ApplyRequestedState();

    private void ApplyRequestedState()
    {
        var uri = new Uri(Navigation.Uri);
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (!parts[0].Equals("state", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ShowState(parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty);
            return;
        }
    }

    private void ShowState(string state)
    {
        switch (state.ToLowerInvariant())
        {
            case "existing":
                ShowExisting();
                break;
            case "form":
                ShowCreate();
                break;
            case "delete":
                ShowDelete();
                break;
            default:
                ShowEmpty();
                break;
        }
    }

    private void ShowEmpty()
    {
        reviewState = ProfileReviewState.Empty;
        isEditMode = false;
        showDeleteSnackbar = false;
    }

    private void ShowExisting()
    {
        reviewState = ProfileReviewState.Existing;
        isEditMode = false;
        showDeleteSnackbar = false;
    }

    private void ShowCreate()
    {
        reviewState = ProfileReviewState.Form;
        isEditMode = false;
        showDeleteSnackbar = false;
    }

    private void ShowEdit()
    {
        reviewState = ProfileReviewState.Form;
        isEditMode = true;
        showDeleteSnackbar = false;
    }

    private void ShowDelete()
    {
        reviewState = ProfileReviewState.Existing;
        isEditMode = false;
        snackbarClosing = false;
        showDeleteSnackbar = true;
    }

    private string SnackbarClass => snackbarClosing ? "company-snackbar is-closing" : "company-snackbar";

    private async Task CloseDeleteSnackbar() => await DismissSnackbar(ShowExisting);

    private async Task ConfirmDelete() => await DismissSnackbar(ShowEmpty);

    private async Task DismissSnackbar(Action afterClose)
    {
        if (snackbarClosing)
        {
            return;
        }

        snackbarClosing = true;
        await Task.Delay(220);
        afterClose();
        snackbarClosing = false;
    }

    private string FormValue(string value) => isEditMode ? value : string.Empty;
}
