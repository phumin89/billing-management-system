using BillingManagement.Client.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace BillingManagement.UnitTests.Layout;

public sealed class MainLayoutBehaviorTests
{
    [Fact]
    public async Task Company_profile_query_state_keeps_company_navigation_active()
    {
        using var services = new ServiceCollection()
            .AddSingleton<NavigationManager>(new TestNavigationManager())
            .AddSingleton<IJSRuntime, TestJsRuntime>()
            .BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);

        var markup = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(MainLayout.Body)] = (RenderFragment)(_ => { })
            });
            var component = await renderer.RenderComponentAsync<MainLayout>(parameters);
            return component.ToHtmlString();
        });

        Assert.Contains("sidebar active-company", markup);
        Assert.Contains("company-link active", markup);
        Assert.Contains("aria-current=\"page\"", markup);
    }

    [Fact]
    public void Company_navigation_reveal_tracks_late_geometry_changes_without_a_timer()
    {
        var script = ReadClientFile(Path.Combine("Layout", "MainLayout.razor.js"));

        Assert.Contains("new ResizeObserver", script, StringComparison.Ordinal);
        Assert.Contains("revealStates.get(element)", script, StringComparison.Ordinal);
        Assert.Contains("observer.observe(container)", script, StringComparison.Ordinal);
        Assert.Contains("observer.observe(child)", script, StringComparison.Ordinal);
        Assert.Contains("container.scrollLeft += Math.ceil", script, StringComparison.Ordinal);
        Assert.Contains("observer.disconnect()", script, StringComparison.Ordinal);
        Assert.DoesNotContain("element.scrollIntoView", script, StringComparison.Ordinal);
        Assert.DoesNotContain("setTimeout", script, StringComparison.Ordinal);
    }

    private static string ReadClientFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine(
                directory.FullName,
                "src",
                "BillingManagement.Client",
                relativePath);

            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find client file '{relativePath}'.");
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
