using System.Net;
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
            markup.IndexOf("company-identity-row", StringComparison.Ordinal));
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
        var markup = WebUtility.HtmlDecode(await RenderExistingProfile());

        Assert.Contains("Change cover", markup);
        Assert.Contains("Reset cover", markup);
        Assert.Contains("accept=\"image/png,image/jpeg,image/webp\"", markup);
    }

    [Fact]
    public async Task Existing_profile_renders_icon_upload_and_reset_controls_near_the_icon()
    {
        var markup = WebUtility.HtmlDecode(await RenderExistingProfile());

        Assert.Contains("Change icon", markup);
        Assert.Contains("Reset icon", markup);
        Assert.Contains("company-icon-picker", markup);
        Assert.True(
            markup.IndexOf("company-identity-icon", StringComparison.Ordinal) <
            markup.IndexOf("Change icon", StringComparison.Ordinal));
        Assert.True(
            markup.IndexOf("Change icon", StringComparison.Ordinal) <
            markup.IndexOf("company-identity-copy", StringComparison.Ordinal));
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
