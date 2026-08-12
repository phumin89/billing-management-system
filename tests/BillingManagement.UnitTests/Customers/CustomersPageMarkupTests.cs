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
        Assert.Contains("<PageTitle>Billing Management | Customers</PageTitle>", markup);
        Assert.Contains("<h1 tabindex=\"-1\">Customers</h1>", markup);
        Assert.Contains("<table", markup);
        Assert.Contains("Customer name", markup);
        Assert.Contains("Email", markup);
        Assert.Contains("Phone", markup);
        Assert.Contains("class=\"document-filter customer-filter\"", markup);
        Assert.Contains("class=\"document-pagination\"", markup);
        Assert.Contains("No customers yet", markup);
        Assert.Equal(2, Count(markup, "href=\"/customers/create\""));
        Assert.True(markup.IndexOf("</table>", StringComparison.Ordinal) < markup.IndexOf("No customers yet", StringComparison.Ordinal));
        Assert.DoesNotContain("@inject", markup);
        Assert.DoesNotContain("@code", markup);
    }

    [Fact]
    public void Customers_page_defines_same_page_api_edit_mode()
    {
        var markup = ReadClientFile("Pages", "Customers", "Customers.razor");
        var codeBehind = ReadClientFile("Pages", "Customers", "Customers.razor.cs");

        Assert.Contains("@onclick=\"() => BeginEdit(customer)\"", markup);
        Assert.Contains("@if (IsEditing)", markup);
        Assert.Contains("<CustomerFormFields Form=\"editForm!\" FieldError=\"FieldError\" />", markup);
        Assert.Contains("@onclick=\"CancelEdit\"", markup);
        Assert.Contains("@onsubmit=\"SaveCustomer\"", markup);
        Assert.Contains("type=\"submit\" disabled=\"@isSubmitting\"", markup);
        Assert.Contains("CustomerClient", codeBehind);
        Assert.DoesNotContain("HttpClient", codeBehind);
    }

    [Fact]
    public void Customers_page_defines_loading_failure_and_retry_states()
    {
        var markup = ReadClientFile("Pages", "Customers", "Customers.razor");

        Assert.Contains("Loading customers", markup);
        Assert.Contains("role=\"status\"", markup);
        Assert.Contains("Could not load customers", markup);
        Assert.Contains("role=\"alert\"", markup);
        Assert.Contains("@onclick=\"RetryLoad\"", markup);
    }

    [Fact]
    public void Customer_create_page_defines_bound_submit_form_and_local_navigation()
    {
        var markup = ReadClientFile("Pages", "Customers", "CreateCustomer.razor");

        Assert.Contains("@page \"/customers/create\"", markup);
        Assert.Contains("<CustomerFormFields Form=\"form\" FieldError=\"FieldError\" />", markup);

        var fieldsMarkup = ReadClientFile("Components", "Customers", "CustomerFormFields.razor");

        Assert.Contains("for=\"customer-name\"", fieldsMarkup);
        Assert.Contains("class=\"required-marker\"", fieldsMarkup);

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
            Assert.Contains($"id=\"{fieldId}\"", fieldsMarkup);
        }

        Assert.Contains("@onsubmit=\"SaveCustomer\"", markup);
        Assert.Contains("novalidate", markup);
        Assert.Contains("@bind=\"Form.CustomerName\"", fieldsMarkup);
        Assert.Contains("@bind:event=\"oninput\"", fieldsMarkup);
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
        var styles = ReadClientFile("wwwroot", "css", "_base.scss");
        var normalizedStyles = styles.ReplaceLineEndings("\n");

        Assert.Contains(".customer-form-field,\n.company-field", normalizedStyles);
        Assert.Contains(".customer-form-grid,\n.company-form-grid", normalizedStyles);
        Assert.Contains("grid-template-columns: repeat(2, minmax(0, 1fr));", normalizedStyles);
    }

    [Fact]
    public void Customers_styles_keep_full_width_mobile_action_inside_page()
    {
        var styles = ReadClientFile("wwwroot", "css", "_base.scss").ReplaceLineEndings("\n");

        Assert.Contains(".primary-link,\n  .customers-create-button {\n    width: 100%;\n  }", styles);
    }

    [Fact]
    public void Customers_page_defines_delete_confirmation_and_pending_guard()
    {
        var markup = ReadClientFile("Pages", "Customers", "Customers.razor");

        Assert.Contains("@onclick=\"() => BeginDelete(customer)\"", markup);
        Assert.Contains("Delete customer?", markup);
        Assert.Contains("Customers already used by a quotation or invoice cannot be deleted.", markup);
        Assert.Contains("@onclick=\"CloseDeleteSnackbar\"", markup);
        Assert.Contains("disabled=\"@isDeleting\"", markup);
        Assert.Contains("Deleting...", markup);
    }

    [Fact]
    public void Customers_delete_confirmation_uses_vertical_motion_and_reduced_motion_override()
    {
        var styles = ReadClientFile("wwwroot", "css", "_base.scss")
            .ReplaceLineEndings("\n");

        Assert.Contains(".customer-delete-snackbar,\n.company-snackbar", styles);
        Assert.Contains("@media (prefers-reduced-motion: reduce)", styles);
        Assert.Contains("transition-duration: 0.01ms !important;", styles);
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
