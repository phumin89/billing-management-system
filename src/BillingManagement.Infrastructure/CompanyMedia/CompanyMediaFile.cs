namespace BillingManagement.Infrastructure.CompanyMedia;

internal sealed class CompanyMediaFile
{
    private CompanyMediaFile()
    {
    }

    public Guid Id { get; private set; }

    public byte[] Content { get; private set; } = [];

    public long Length { get; private set; }

    public byte[] Version { get; private set; } = [];

    public static CompanyMediaFile Create(Guid id, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new CompanyMediaFile
        {
            Id = id,
            Content = content,
            Length = content.LongLength
        };
    }

    public void ReplaceContent(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        this.Content = content;
        this.Length = content.LongLength;
    }
}
