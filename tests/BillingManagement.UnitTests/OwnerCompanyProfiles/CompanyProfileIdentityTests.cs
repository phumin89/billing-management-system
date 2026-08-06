using System.Net;
using System.Reflection;
using BillingManagement.Client.OwnerCompanyProfiles;
using BillingManagement.Client.Pages.CompanyProfile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class CompanyProfileIdentityTests
{
    [Fact]
    public async Task Existing_profile_renders_approved_identity_hierarchy()
    {
        var markup = await RenderExistingProfile();

        Assert.Contains(">Company profile</h1>", markup);
        Assert.Contains("Manage company identity and contact details.", markup);
        Assert.DoesNotContain("Owner company", markup);
        Assert.Contains("src=\"images/company-profile/header.svg\"", markup);
        Assert.Contains("width=\"1200\" height=\"440\"", markup);
        Assert.Contains("src=\"images/company-profile/company-icon.svg\"", markup);
        Assert.Contains("alt=\"mi.nie company mark\"", markup);
        Assert.True(
            markup.IndexOf("company-identity-cover", StringComparison.Ordinal) <
            markup.IndexOf("company-identity-band", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Existing_profile_renders_actionable_contact_links()
    {
        var markup = WebUtility.HtmlDecode(await RenderExistingProfile());

        Assert.Contains("class=\"company-contact-link\" href=\"mailto:billing@acme.example\"", markup);
        Assert.Contains("class=\"company-contact-link\" href=\"tel:+6625550100\"", markup);
        Assert.Contains("aria-label=\"Email billing@acme.example\"", markup);
        Assert.Contains("aria-label=\"Call +66 2 555 0100\"", markup);
    }

    [Fact]
    public async Task Existing_profile_renders_cover_upload_and_reset_controls()
    {
        _ = await RenderExistingProfile();
        var markup = ReadCompanyProfileMarkup();

        Assert.Contains("Choose cover", markup);
        Assert.Contains("Reset cover", markup);
        Assert.Contains("accept=\"image/png,image/jpeg,image/webp\"", markup);
    }

    [Fact]
    public async Task Existing_profile_renders_icon_upload_and_reset_controls_in_media_tray()
    {
        _ = await RenderExistingProfile();
        var markup = ReadCompanyProfileMarkup();

        Assert.Contains("Choose icon", markup);
        Assert.Contains("Company icon", markup);
        Assert.Contains("company-icon-picker", markup);
        Assert.True(
            markup.IndexOf("company-identity-icon", StringComparison.Ordinal) <
            markup.IndexOf("company-media-tray", StringComparison.Ordinal));
    }

    [Fact]
    public void Company_profile_uses_identity_band_and_progressive_media_controls()
    {
        var markup = ReadCompanyProfileMarkup();

        Assert.Contains("class=\"company-identity-band\"", markup);
        Assert.Contains("@onclick=\"ToggleMediaTray\"", markup);
        Assert.Contains("@if (showMediaTray", markup);
        Assert.Contains("class=\"company-media-tray\"", markup);
        Assert.True(
            markup.IndexOf("company-identity-cover", StringComparison.Ordinal) <
            markup.IndexOf("company-identity-band", StringComparison.Ordinal));
    }

    [Fact]
    public void Existing_profile_keeps_update_primary_and_delete_in_overflow_menu()
    {
        var markup = ReadCompanyProfileMarkup();

        Assert.Contains("class=\"company-primary-button\" type=\"button\" @onclick=\"ShowEdit\">Update", markup);
        Assert.Contains("class=\"company-overflow-menu\" open=\"@showOverflowMenu\"", markup);
        Assert.Contains("@onclick:preventDefault @onclick=\"ToggleOverflowMenu\"", markup);
        Assert.Contains("@onclick=\"ShowDelete\">Delete company profile", markup);
    }

    [Fact]
    public void Delete_confirmation_closes_the_overflow_menu()
    {
        var component = new CompanyProfile();
        var toggle = typeof(CompanyProfile).GetMethod(
            "ToggleOverflowMenu",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var showDelete = typeof(CompanyProfile).GetMethod(
            "ShowDelete",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var menuField = typeof(CompanyProfile).GetField(
            "showOverflowMenu",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var snackbarField = typeof(CompanyProfile).GetField(
            "showDeleteSnackbar",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        toggle.Invoke(component, null);
        Assert.True((bool)menuField.GetValue(component)!);

        showDelete.Invoke(component, null);

        Assert.False((bool)menuField.GetValue(component)!);
        Assert.True((bool)snackbarField.GetValue(component)!);
    }

    [Fact]
    public void Form_groups_fields_and_keeps_email_optional()
    {
        var markup = ReadCompanyProfileMarkup();

        Assert.Contains(">Company identity</h3>", markup);
        Assert.Contains(">Address</h3>", markup);
        Assert.Contains(">Contact and references</h3>", markup);
        Assert.Contains("<label for=\"email\">Email</label>", markup);
        Assert.DoesNotContain("<label for=\"email\">Email*</label>", markup);
    }

    [Fact]
    public void Approved_radius_and_accessibility_rules_are_scoped_in_styles()
    {
        var styles = ReadCompanyProfileStyles().ReplaceLineEndings("\n");

        Assert.Contains(".company-page-header {\n  margin-bottom: 24px;\n  user-select: none;", styles);
        Assert.Contains(".company-card {", styles);
        Assert.Contains("border-radius: 4px;", styles);
        Assert.Contains(".company-identity-icon {", styles);
        Assert.Contains("border-radius: 8px;", styles);
        Assert.Contains(".company-snackbar {", styles);
        Assert.Contains("border-radius: 6px;", styles);
        Assert.Contains(".company-overflow-menu summary {", styles);
        Assert.Contains("box-sizing: border-box;", styles);
        Assert.Contains(".company-contact-link,\n.company-contact-link:visited", styles);
        Assert.Contains("text-decoration: none;", styles);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", styles);
    }

    [Fact]
    public void Media_tray_toggle_opens_and_closes_inline_controls()
    {
        var component = new CompanyProfile();
        var toggle = typeof(CompanyProfile).GetMethod(
            "ToggleMediaTray",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var field = typeof(CompanyProfile).GetField(
            "showMediaTray",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        toggle.Invoke(component, null);
        Assert.True((bool)field.GetValue(component)!);

        toggle.Invoke(component, null);
        Assert.False((bool)field.GetValue(component)!);
    }

    [Fact]
    public void Route_focused_heading_does_not_draw_control_outline()
    {
        var styles = ReadApplicationStyles().ReplaceLineEndings("\n");

        Assert.Contains(
            "h1[tabindex=\"-1\"]:focus-visible {\n  outline: none;\n}",
            styles);
    }

    [Fact]
    public void Cover_picker_styles_reach_the_native_file_input()
    {
        var styles = ReadCompanyProfileStyles();

        Assert.Contains(".company-cover-picker ::deep input", styles);
        Assert.Contains(".company-cover-picker:focus-within", styles);
    }

    [Fact]
    public void Icon_picker_styles_reach_the_native_file_input()
    {
        var styles = ReadCompanyProfileStyles();

        Assert.Contains(".company-icon-picker ::deep input", styles);
        Assert.Contains(".company-icon-picker:focus-within", styles);
    }

    private static async Task<string> RenderExistingProfile()
    {
        using var services = new ServiceCollection()
            .AddSingleton<NavigationManager>(new TestNavigationManager())
            .AddSingleton(new OwnerCompanyProfileClient(new HttpClient()))
            .AddSingleton<IJSRuntime, TestJsRuntime>()
            .BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var component = await renderer.RenderComponentAsync<CompanyProfile>();
            return component.ToHtmlString();
        });
    }

    private static string ReadApplicationStyles()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine(
                directory.FullName,
                "src",
                "BillingManagement.Client",
                "wwwroot",
                "css",
                "app.css");

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find application styles.");
    }

    private static string ReadCompanyProfileStyles()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine(
                directory.FullName,
                "src",
                "BillingManagement.Client",
                "Pages",
                "CompanyProfile",
                "CompanyProfile.razor.scss");

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find Company profile styles.");
    }

    private static string ReadCompanyProfileMarkup()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine(
                directory.FullName,
                "src",
                "BillingManagement.Client",
                "Pages",
                "CompanyProfile",
                "CompanyProfile.razor");

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find Company profile markup.");
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            this.Initialize(
                "http://localhost/",
                "http://localhost/company-profile?state=existing");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
        }
    }

    private sealed class TestJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            ValueTask.FromResult(default(TValue)!);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args) =>
            ValueTask.FromResult(default(TValue)!);
    }
}
