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
        Assert.DoesNotContain("company-identity-cover", markup);
        Assert.DoesNotContain("Manage cover image", markup);
        Assert.Contains("src=\"images/company-profile/company-icon.svg\"", markup);
        Assert.Contains("alt=\"mi.nie company mark\"", markup);
        Assert.Contains("Company details used on quotations and invoices.", markup);
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
    public async Task Existing_profile_renders_only_edit_and_delete_actions()
    {
        var markup = WebUtility.HtmlDecode(await RenderExistingProfile());

        Assert.Contains(">Edit</button>", markup);
        Assert.Contains(">Delete</button>", markup);
        Assert.DoesNotContain("Edit profile", markup);
        Assert.DoesNotContain("Delete profile", markup);
        Assert.DoesNotContain("Manage logo", markup);
        Assert.Equal(2, markup.Split("<button", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void Route_focused_heading_does_not_draw_control_outline()
    {
        var styles = ReadApplicationStyles().ReplaceLineEndings("\n");

        Assert.Contains(
            "h1[tabindex=\"-1\"]:focus-visible {",
            styles);
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
