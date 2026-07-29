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
        Assert.Equal(2, Count(markup, "href=\"/customers/create\""));
        Assert.True(markup.IndexOf("</table>", StringComparison.Ordinal) < markup.IndexOf("No customers yet", StringComparison.Ordinal));
        Assert.DoesNotContain("@inject", markup);
        Assert.DoesNotContain("@code", markup);
    }

    [Fact]
    public void Customer_create_page_defines_bound_submit_form_and_local_navigation()
    {
        var markup = ReadClientFile("Pages", "Customers", "CreateCustomer.razor");

        Assert.Contains("@page \"/customers/create\"", markup);
        Assert.Contains("for=\"customer-name\"", markup);
        Assert.Contains("class=\"required-marker\"", markup);

        var fieldIds = new[]
        {
            "customer-name",
            "tax-id",
            "email",
            "phone",
            "billing-address-line-1",
            "billing-address-line-2",
            "city-province-state",
            "postal-code",
            "country",
            "contact-name",
            "notes"
        };

        foreach (var fieldId in fieldIds)
        {
            Assert.Contains($"id=\"{fieldId}\"", markup);
        }

        Assert.Contains("@onsubmit=\"SaveCustomer\"", markup);
        Assert.Contains("novalidate", markup);
        Assert.Contains("@bind=\"form.CustomerName\"", markup);
        Assert.Contains("@bind:event=\"oninput\"", markup);
        Assert.Contains("type=\"submit\"", markup);
        Assert.Contains("disabled=\"@isSubmitting\"", markup);
        Assert.Contains("href=\"/customers\"", markup);
        Assert.DoesNotContain("<EditForm", markup);
        Assert.DoesNotContain("@inject", markup);
        Assert.DoesNotContain("@code", markup);
    }

    [Fact]
    public void Customer_create_styles_keep_labels_and_controls_within_layout()
    {
        var styles = ReadClientFile("Pages", "Customers", "CreateCustomer.razor.scss");
        var normalizedStyles = styles.ReplaceLineEndings("\n");

        Assert.Contains(".customer-form-field label {\n  display: inline-flex;", normalizedStyles);
        Assert.Contains("box-sizing: border-box;", normalizedStyles);
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
