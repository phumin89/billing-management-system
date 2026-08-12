using System.Reflection;
using PdfSharp.Fonts;

namespace BillingManagement.Api.BillingDocuments;

public sealed class BillingDocumentFontResolver : IFontResolver
{
    private const string RegularFace = "DM Sans Regular";
    private const string BoldFace = "DM Sans Bold";

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        return new FontResolverInfo(isBold ? BoldFace : RegularFace);
    }

    public byte[] GetFont(string faceName)
    {
        var resource = faceName == BoldFace
            ? "BillingManagement.Api.Fonts.dm-sans-bold.ttf"
            : "BillingManagement.Api.Fonts.dm-sans-regular.ttf";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Embedded font '{resource}' was not found.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
