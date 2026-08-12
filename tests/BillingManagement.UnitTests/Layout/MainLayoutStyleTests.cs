namespace BillingManagement.UnitTests.Layout;

public sealed class MainLayoutStyleTests
{
    [Fact]
    public void Layout_uses_registry_rail_and_collapsed_content_offset()
    {
        var styles = ReadStyles();

        Assert.Contains("--rail: #ffffff;", styles, StringComparison.Ordinal);
        Assert.Contains("--signature-coral: #aa2d00;", styles, StringComparison.Ordinal);
        Assert.Contains(".sidebar {", styles, StringComparison.Ordinal);
        Assert.Contains("position: fixed;", styles, StringComparison.Ordinal);
        Assert.Contains("background: var(--rail);", styles, StringComparison.Ordinal);
        Assert.Contains(".app-shell.is-collapsed .sidebar", styles, StringComparison.Ordinal);
        Assert.Contains(".app-shell.is-collapsed .content-shell", styles, StringComparison.Ordinal);
    }

    [Fact]
    public void Layout_has_keyboard_reduced_motion_and_mobile_navigation_rules()
    {
        var styles = ReadStyles();

        Assert.Contains(":focus-visible", styles, StringComparison.Ordinal);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", styles, StringComparison.Ordinal);
        Assert.Contains("transition-duration: 0.01ms", styles, StringComparison.Ordinal);
        Assert.Contains("@media (max-width: 720px)", styles, StringComparison.Ordinal);
        Assert.Contains("inset: auto 0 0", styles, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: repeat(3, 1fr)", styles, StringComparison.Ordinal);
    }

    [Fact]
    public void Dashboard_does_not_mark_a_feature_navigation_item_active()
    {
        var layout = ReadClientFile("Layout", "MainLayout.razor");

        Assert.Contains("if (path.StartsWith(\"customers\"", layout, StringComparison.Ordinal);
        Assert.Contains("return string.Empty;", layout, StringComparison.Ordinal);
    }

    private static string ReadStyles()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, "src", "BillingManagement.Client", "wwwroot", "css", "app.css");
            if (File.Exists(path))
            {
                return File.ReadAllText(path).ReplaceLineEndings("\n");
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find generated client stylesheet.");
    }

    private static string ReadClientFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, "src", "BillingManagement.Client", .. segments]);
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find client file.");
    }
}
