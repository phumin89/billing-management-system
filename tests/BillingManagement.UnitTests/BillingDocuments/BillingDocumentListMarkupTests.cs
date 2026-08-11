namespace BillingManagement.UnitTests.BillingDocuments;

public sealed class BillingDocumentListMarkupTests
{
    [Theory]
    [InlineData("Quotations", "Quotation list")]
    [InlineData("Invoices", "Invoice list")]
    public void Register_uses_an_accessible_table(string feature, string accessibleName)
    {
        var markup = ReadClientFile("Pages", feature, $"{feature}.razor");

        Assert.Contains($"aria-label=\"{accessibleName}\"", markup);
        Assert.Contains("class=\"document-register\"", markup);
        Assert.Contains("class=\"document-register-header\"", markup);
        Assert.Contains("class=\"document-register-count\"", markup);
        Assert.Contains("<table class=\"document-table\">", markup);
        Assert.Contains("<th scope=\"col\">Reference</th>", markup);
        Assert.Contains("<th scope=\"col\">Customer</th>", markup);
        Assert.Contains("<th scope=\"col\">Issued</th>", markup);
        Assert.Contains("<th scope=\"col\">Value</th>", markup);
        Assert.DoesNotContain("document-list-item", markup);
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
