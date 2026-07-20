using System.Text.RegularExpressions;

namespace BillingManagement.UnitTests.Layout;

public sealed class MainLayoutStyleTests
{
    [Fact]
    public void Sidebar_rows_share_the_fixed_icon_grid()
    {
        var styles = ReadStyles();
        var sidebar = Rule(styles, @"^\.sidebar");
        var rows = Rule(styles, @"^\.brand,\s*\.nav-item,\s*\.company-link");

        Assert.Equal("28px 14px 14px", Property(sidebar, "padding"));
        Assert.Equal("grid", Property(rows, "display"));
        Assert.Equal("42px minmax(0, 1fr)", Property(rows, "grid-template-columns"));
        Assert.Equal("14px", Property(rows, "column-gap"));
        Assert.Equal("21px", Property(rows, "padding-inline"));
        Assert.Equal("stretch", Property(rows, "justify-content"));
        Assert.Equal("hidden", Property(rows, "overflow"));
    }

    [Fact]
    public void Sidebar_motion_does_not_animate_layout_inside_rows()
    {
        var styles = ReadStyles();
        var sidebar = Rule(styles, @"^\.sidebar");
        var label = Rule(styles, @"^\.sidebar-text");
        var collapsedLabel = Rule(styles, @"^\.app-shell\.is-collapsed \.sidebar-text");

        Assert.Equal("width 300ms ease-out", Property(sidebar, "transition"));
        Assert.Equal(
            "opacity 300ms ease-out, transform 300ms ease-out",
            Property(label, "transition"));
        Assert.DoesNotContain("max-width", label, StringComparison.Ordinal);
        Assert.Equal("0", Property(collapsedLabel, "opacity"));
        Assert.Equal("translateX(-8px)", Property(collapsedLabel, "transform"));
        Assert.DoesNotContain("max-width", collapsedLabel, StringComparison.Ordinal);
    }

    [Fact]
    public void Sidebar_edges_content_and_reduced_motion_stay_stable()
    {
        var styles = ReadStyles();
        var indicator = Rule(styles, @"^\.sidebar-indicator");
        var toggle = Rule(styles, @"^\.sidebar-toggle");
        var toggleIcon = Rule(styles, @"^\.sidebar-toggle svg");
        var company = LastRule(styles, @"^\.company-link");
        var content = Rule(styles, @"^\.content-shell");

        Assert.Equal("16px", Property(indicator, "left"));
        Assert.Equal("15px", Property(indicator, "right"));
        Assert.Equal("-32px", Property(toggle, "right"));
        Assert.Equal("transform 300ms ease-out", Property(toggleIcon, "transition"));
        Assert.Equal("0", Property(company, "border"));
        Assert.Contains("inset 0 0 0 1px #dddddd", Property(company, "box-shadow"));
        Assert.Equal("316px", Property(content, "margin-left"));
        Assert.Contains("@media (prefers-reduced-motion: reduce)", styles, StringComparison.Ordinal);
        Assert.Contains(
            ".sidebar,\n  .sidebar-indicator,\n  .sidebar-text,\n  .sidebar-toggle svg {\n    transition: none;\n  }",
            styles,
            StringComparison.Ordinal);
    }

    private static string ReadStyles()
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

        throw new FileNotFoundException("Could not find generated client stylesheet.");
    }

    private static string Rule(string styles, string selectorPattern)
    {
        var match = Regex.Match(
            styles,
            $@"{selectorPattern}\s*\{{(?<body>[^}}]*)\}}",
            RegexOptions.Multiline);

        Assert.True(match.Success, $"Could not find CSS rule matching '{selectorPattern}'.");
        return match.Groups["body"].Value;
    }

    private static string LastRule(string styles, string selectorPattern)
    {
        var matches = Regex.Matches(
            styles,
            $@"{selectorPattern}\s*\{{(?<body>[^}}]*)\}}",
            RegexOptions.Multiline);

        Assert.NotEmpty(matches);
        return matches[^1].Groups["body"].Value;
    }

    private static string Property(string rule, string property)
    {
        var match = Regex.Match(
            rule,
            $@"(?:^|;)\s*{Regex.Escape(property)}\s*:\s*(?<value>[^;]+);",
            RegexOptions.Multiline);

        Assert.True(match.Success, $"Could not find CSS property '{property}'.");
        return Regex.Replace(match.Groups["value"].Value, @"\s+", " ").Trim();
    }
}
