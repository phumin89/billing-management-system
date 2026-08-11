namespace BillingManagement.UnitTests.BillingDocuments;

public sealed class BillingDocumentDetailMarkupTests
{
    [Theory]
    [InlineData("Quotations", "QuotationDetail.razor")]
    [InlineData("Invoices", "InvoiceDetail.razor")]
    public void Document_header_renders_the_stable_company_mark(string feature, string fileName)
    {
        var markup = ReadClientFile("Pages", feature, fileName);

        Assert.Contains("class=\"document-issuer\"", markup);
        Assert.Contains("src=\"/images/company-profile/company-icon.svg\"", markup);
        Assert.Contains("alt=\"Company logo\"", markup);
        Assert.DoesNotContain("company cover", markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Quotation_has_preparer_and_customer_acceptance_signatures()
    {
        var markup = ReadClientFile("Pages", "Quotations", "QuotationDetail.razor");

        Assert.Contains("Prepared by", markup);
        Assert.Contains("Accepted by", markup);
    }

    [Fact]
    public void Invoice_has_authorized_issuer_signature()
    {
        var markup = ReadClientFile("Pages", "Invoices", "InvoiceDetail.razor");

        Assert.Contains("Authorized by", markup);
        Assert.DoesNotContain("Accepted by", markup);
    }

    [Theory]
    [InlineData("Quotations", "QuotationDetail.razor")]
    [InlineData("Invoices", "InvoiceDetail.razor")]
    public void Document_quantity_omits_insignificant_trailing_zeroes(string feature, string fileName)
    {
        var markup = ReadClientFile("Pages", feature, fileName);

        Assert.Contains("Quantity.ToString(\"0.##\")", markup);
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
