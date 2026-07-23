namespace BillingManagement.UnitTests.Customers;

public sealed class CustomersPageMarkupTests
{
    [Fact]
    public void Customers_page_defines_empty_state_table_layout_and_create_actions()
    {
        var path = FindClientFile("Pages", "Customers", "Customers.razor");

        Assert.NotNull(path);

        var markup = File.ReadAllText(path);

        Assert.Contains("@page \"/customers\"", markup);
        Assert.Contains("<h1 tabindex=\"-1\">Customers</h1>", markup);
        Assert.Contains("<table", markup);
        Assert.Contains("Customer name", markup);
        Assert.Contains("Email", markup);
        Assert.Contains("Phone", markup);
        Assert.Contains("No customers yet", markup);
        Assert.Equal(2, Count(markup, ">Create customer</button>"));
        Assert.True(markup.IndexOf("</table>", StringComparison.Ordinal) < markup.IndexOf("No customers yet", StringComparison.Ordinal));
        Assert.DoesNotContain("@inject", markup);
        Assert.DoesNotContain("@code", markup);
    }

    [Fact]
    public void Primary_navigation_links_to_customers_page()
    {
        var layout = ReadClientFile("Layout", "MainLayout.razor");

        Assert.Contains("href=\"/customers\"", layout);
    }

    private static string ReadClientFile(params string[] segments)
    {
        var path = FindClientFile(segments);

        Assert.NotNull(path);
        return File.ReadAllText(path);
    }

    private static string? FindClientFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var path = Path.Combine([directory.FullName, "src", "BillingManagement.Client", .. segments]);
            if (File.Exists(path))
            {
                return path;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static int Count(string value, string fragment) =>
        value.Split(fragment, StringSplitOptions.None).Length - 1;
}
