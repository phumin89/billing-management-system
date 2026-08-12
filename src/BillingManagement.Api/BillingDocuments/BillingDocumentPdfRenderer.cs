using System.Reflection;
using BillingManagement.Application.Abstractions.BillingDocuments;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace BillingManagement.Api.BillingDocuments;

public sealed class BillingDocumentPdfRenderer
{
    private const string FontFamily = "DM Sans";
    private static readonly Color Ink = Color.Parse("#181D26");
    private static readonly Color SoftSurface = Color.Parse("#F8FAFC");
    private static readonly string CompanyIcon = LoadCompanyIcon();

    static BillingDocumentPdfRenderer()
    {
        GlobalFontSettings.FontResolver ??= new BillingDocumentFontResolver();
    }

    public byte[] Render(QuotationRecord quotation)
    {
        return this.RenderDocument(new DocumentContent(
            "QUOTATION", quotation.Number, quotation.SellerCompanyName, quotation.SellerAddress,
            quotation.SellerTaxId, quotation.SellerPhone, quotation.SellerEmail, quotation.SellerWebsite,
            quotation.SellerRegistrationNumber,
            "PREPARED FOR", quotation.CustomerName, quotation.CustomerAddress, quotation.CustomerTaxId,
            "ISSUE DATE", quotation.IssueDate, "VALID UNTIL", quotation.ValidUntil, quotation.Currency,
            quotation.Items, quotation.Subtotal, quotation.TaxTotal, quotation.Total, "Total",
            ["PREPARED BY", "ACCEPTED BY"], null));
    }

    public byte[] Render(InvoiceRecord invoice)
    {
        return this.RenderDocument(new DocumentContent(
            "INVOICE", invoice.Number, invoice.SellerCompanyName, invoice.SellerAddress,
            invoice.SellerTaxId, invoice.SellerPhone, invoice.SellerEmail, invoice.SellerWebsite,
            invoice.SellerRegistrationNumber,
            "BILL TO", invoice.CustomerName, invoice.CustomerAddress, invoice.CustomerTaxId,
            "ISSUE DATE", invoice.IssueDate, "PAYMENT DUE", invoice.DueDate, invoice.Currency,
            invoice.Items, invoice.Subtotal, invoice.TaxTotal, invoice.Total, "Total due",
            ["AUTHORIZED BY"], invoice.DisplayStatus(DateOnly.FromDateTime(DateTime.UtcNow)).ToString().ToUpperInvariant()));
    }

    private byte[] RenderDocument(DocumentContent content)
    {
        var document = this.CreateDocument(content);
        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();
        renderer.PdfDocument.Info.Creator = "Billing Management";
        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    private Document CreateDocument(DocumentContent content)
    {
        var document = new Document();
        ConfigureDocumentInfo(document, content);
        this.ConfigureStyles(document);
        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromMillimeter(18);
        section.PageSetup.BottomMargin = Unit.FromMillimeter(18);
        section.PageSetup.LeftMargin = Unit.FromMillimeter(18);
        section.PageSetup.RightMargin = Unit.FromMillimeter(18);
        AddPageFooter(section);
        this.AddHeader(section, content);
        this.AddCustomer(section, content);
        this.AddItems(section, content);
        this.AddTotals(section, content);
        this.AddSignatures(section, content.Signatures);
        return document;
    }

    private void ConfigureStyles(Document document)
    {
        var normal = document.Styles[StyleNames.Normal]!;
        normal.Font.Name = FontFamily;
        normal.Font.Size = Unit.FromPoint(9.5);
        normal.Font.Color = Ink;
        normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(3);
    }

    private void AddHeader(Section section, DocumentContent content)
    {
        var table = section.AddTable();
        table.AddColumn(Unit.FromCentimeter(9));
        table.AddColumn(Unit.FromCentimeter(8));
        var row = table.AddRow();
        var title = row.Cells[0].AddParagraph(content.Type);
        title.Format.Font.Size = Unit.FromPoint(26);
        title.Format.Font.Bold = true;
        var reference = row.Cells[0].AddParagraph($"# {content.Number}");
        reference.Format.Font.Size = Unit.FromPoint(8);
        reference.Format.Font.Bold = true;
        if (content.Status is not null)
        {
            var status = row.Cells[0].AddParagraph(content.Status);
            status.Format.SpaceBefore = Unit.FromPoint(9);
            status.Format.Font.Size = Unit.FromPoint(7.5);
            status.Format.Font.Bold = true;
            status.Format.Font.Color = Colors.Gray;
        }
        var seller = row.Cells[1];
        seller.Format.Alignment = ParagraphAlignment.Right;
        var logo = seller.AddParagraph();
        logo.Format.Alignment = ParagraphAlignment.Right;
        logo.Format.SpaceAfter = Unit.FromPoint(8);
        logo.AddImage(CompanyIcon).Height = Unit.FromPoint(34);
        seller.AddParagraph(content.SellerName).Format.Font.Bold = true;
        seller.AddParagraph(content.SellerAddress);
        seller.AddParagraph(JoinContact(content.SellerPhone, content.SellerEmail));
        if (!string.IsNullOrWhiteSpace(content.SellerWebsite)) seller.AddParagraph(content.SellerWebsite);
        if (!string.IsNullOrWhiteSpace(content.SellerTaxId)) seller.AddParagraph($"Tax ID: {content.SellerTaxId}");
        if (!string.IsNullOrWhiteSpace(content.SellerRegistrationNumber))
        {
            seller.AddParagraph($"Registration: {content.SellerRegistrationNumber}");
        }
        var rule = section.AddParagraph();
        rule.Format.SpaceBefore = Unit.FromPoint(22);
        rule.Format.SpaceAfter = Unit.FromPoint(16);
        rule.Format.Borders.Bottom.Width = Unit.FromPoint(1);
        rule.Format.Borders.Bottom.Color = Ink;
    }

    private void AddCustomer(Section section, DocumentContent content)
    {
        var table = section.AddTable();
        table.AddColumn(Unit.FromCentimeter(9));
        table.AddColumn(Unit.FromCentimeter(4));
        table.AddColumn(Unit.FromCentimeter(4));
        var row = table.AddRow();
        AddLabel(row.Cells[0], content.CustomerLabel);
        var customer = row.Cells[0].AddParagraph(content.CustomerName);
        customer.Format.Font.Bold = true;
        customer.Format.SpaceBefore = Unit.FromPoint(8);
        if (!string.IsNullOrWhiteSpace(content.CustomerAddress)) row.Cells[0].AddParagraph(content.CustomerAddress);
        if (!string.IsNullOrWhiteSpace(content.CustomerTaxId)) row.Cells[0].AddParagraph($"Tax ID: {content.CustomerTaxId}");
        AddDate(row.Cells[1], content.FirstDateLabel, content.FirstDate);
        AddDate(row.Cells[2], content.SecondDateLabel, content.SecondDate);
        section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(16);
    }

    private void AddItems(Section section, DocumentContent content)
    {
        var table = section.AddTable();
        table.Borders.Width = 0;
        table.AddColumn(Unit.FromCentimeter(7.2));
        table.AddColumn(Unit.FromCentimeter(1.6));
        table.AddColumn(Unit.FromCentimeter(3));
        table.AddColumn(Unit.FromCentimeter(2.4));
        table.AddColumn(Unit.FromCentimeter(2.8));
        var header = table.AddRow();
        header.HeadingFormat = true;
        header.Borders.Bottom.Width = Unit.FromPoint(1.25);
        header.Borders.Bottom.Color = Ink;
        AddHeaderCell(header.Cells[0], "DESCRIPTION", ParagraphAlignment.Left);
        AddHeaderCell(header.Cells[1], "QTY", ParagraphAlignment.Right);
        AddHeaderCell(header.Cells[2], "UNIT PRICE", ParagraphAlignment.Right);
        AddHeaderCell(header.Cells[3], "TAX", ParagraphAlignment.Right);
        AddHeaderCell(header.Cells[4], "TOTAL", ParagraphAlignment.Right);
        for (var index = 0; index < content.Items.Count; index++)
        {
            var item = content.Items[index];
            var subtotal = decimal.Round(item.Quantity * item.UnitPrice, 2);
            var tax = decimal.Round(subtotal * item.TaxRate / 100, 2);
            var row = table.AddRow();
            if (index % 2 == 0) row.Shading.Color = SoftSurface;
            AddValueCell(row.Cells[0], item.Description, ParagraphAlignment.Left);
            AddValueCell(row.Cells[1], item.Quantity.ToString("0.##"), ParagraphAlignment.Right);
            AddValueCell(row.Cells[2], item.UnitPrice.ToString("N2"), ParagraphAlignment.Right);
            AddValueCell(row.Cells[3], tax.ToString("N2"), ParagraphAlignment.Right);
            AddValueCell(row.Cells[4], (subtotal + tax).ToString("N2"), ParagraphAlignment.Right);
        }
    }

    private void AddTotals(Section section, DocumentContent content)
    {
        var table = section.AddTable();
        table.Rows.LeftIndent = Unit.FromCentimeter(8.4);
        table.AddColumn(Unit.FromCentimeter(4.6));
        table.AddColumn(Unit.FromCentimeter(4));
        AddTotalRow(table, "Subtotal", content.Subtotal.ToString("N2"), false);
        AddTotalRow(table, "Tax", content.TaxTotal.ToString("N2"), false);
        AddTotalRow(table, content.TotalLabel, $"{content.Total:N2} {content.Currency}", true);
        table.Rows[0].KeepWith = 2;
    }

    private void AddSignatures(Section section, IReadOnlyList<string> signatures)
    {
        var table = section.AddTable();
        table.Format.SpaceBefore = Unit.FromPoint(48);
        var width = signatures.Count == 1 ? Unit.FromCentimeter(7) : Unit.FromCentimeter(8);
        if (signatures.Count == 1) table.Rows.LeftIndent = Unit.FromCentimeter(10);
        foreach (var _ in signatures) table.AddColumn(width);
        var row = table.AddRow();
        for (var index = 0; index < signatures.Count; index++)
        {
            AddLabel(row.Cells[index], signatures[index]);
            var line = row.Cells[index].AddParagraph();
            line.Format.SpaceBefore = Unit.FromPoint(18);
            line.Format.SpaceAfter = Unit.FromPoint(4);
            line.Format.Borders.Bottom.Width = Unit.FromPoint(0.8);
            row.Cells[index].AddParagraph("Name / position / date").Format.Font.Size = Unit.FromPoint(7);
        }
    }

    private static void AddDate(Cell cell, string label, DateOnly date)
    {
        cell.VerticalAlignment = VerticalAlignment.Bottom;
        AddLabel(cell, label);
        var value = cell.AddParagraph(date.ToString("dd MMM yyyy"));
        value.Format.Font.Bold = true;
        value.Format.SpaceBefore = Unit.FromPoint(7);
    }

    private static void AddLabel(Cell cell, string value)
    {
        var paragraph = cell.AddParagraph(value);
        paragraph.Format.Font.Size = Unit.FromPoint(7.5);
        paragraph.Format.Font.Bold = true;
    }

    private static void AddHeaderCell(Cell cell, string value, ParagraphAlignment alignment)
    {
        var paragraph = cell.AddParagraph(value);
        paragraph.Format.SpaceBefore = Unit.FromPoint(8);
        paragraph.Format.SpaceAfter = Unit.FromPoint(8);
        paragraph.Format.Alignment = alignment;
        paragraph.Format.Font.Size = Unit.FromPoint(7.5);
        paragraph.Format.Font.Bold = true;
    }

    private static void AddValueCell(Cell cell, string value, ParagraphAlignment alignment)
    {
        var paragraph = cell.AddParagraph(value);
        paragraph.Format.Alignment = alignment;
        paragraph.Format.SpaceBefore = Unit.FromPoint(9);
        paragraph.Format.SpaceAfter = Unit.FromPoint(9);
    }

    private static void AddTotalRow(Table table, string label, string value, bool emphasized)
    {
        var row = table.AddRow();
        row.Cells[0].AddParagraph(label);
        row.Cells[1].AddParagraph(value).Format.Font.Bold = true;
        row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
        row.Cells[0].Format.SpaceBefore = row.Cells[1].Format.SpaceBefore = Unit.FromPoint(8);
        row.Cells[0].Format.SpaceAfter = row.Cells[1].Format.SpaceAfter = Unit.FromPoint(8);
        if (!emphasized) return;
        row.Shading.Color = Ink;
        row.Cells[0].Format.Font.Color = Colors.White;
        row.Cells[1].Format.Font.Color = Colors.White;
        row.Cells[1].Format.Font.Size = Unit.FromPoint(13);
    }

    private static string JoinContact(params string?[] values)
    {
        return string.Join(" | ", values.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static void ConfigureDocumentInfo(Document document, DocumentContent content)
    {
        document.Info.Title = $"{ToTitleCase(content.Type)} {content.Number}";
        document.Info.Subject = $"{ToTitleCase(content.Type)} issued to {content.CustomerName}";
        document.Info.Author = content.SellerName;
        document.Info.Keywords = $"{content.Type.ToLowerInvariant()}, billing, {content.Number}";
    }

    private static void AddPageFooter(Section section)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Right;
        footer.Format.Font.Size = Unit.FromPoint(7);
        footer.Format.Font.Color = Colors.Gray;
        footer.AddText("Page ");
        footer.AddPageField();
        footer.AddText(" of ");
        footer.AddNumPagesField();
    }

    private static string ToTitleCase(string value)
    {
        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }

    private static string LoadCompanyIcon()
    {
        const string resourceName = "BillingManagement.Api.Images.company-icon.png";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded image '{resourceName}' was not found.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return "base64:" + Convert.ToBase64String(buffer.ToArray());
    }

    private sealed record DocumentContent(
        string Type, string Number, string SellerName, string SellerAddress, string? SellerTaxId,
        string? SellerPhone, string? SellerEmail, string? SellerWebsite, string? SellerRegistrationNumber,
        string CustomerLabel,
        string CustomerName, string? CustomerAddress, string? CustomerTaxId, string FirstDateLabel,
        DateOnly FirstDate, string SecondDateLabel, DateOnly SecondDate, string Currency,
        IReadOnlyList<BillingDocumentItemRecord> Items, decimal Subtotal, decimal TaxTotal,
        decimal Total, string TotalLabel, IReadOnlyList<string> Signatures, string? Status);
}
