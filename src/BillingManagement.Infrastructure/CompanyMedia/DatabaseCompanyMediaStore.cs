using BillingManagement.Application.Abstractions.CompanyMedia;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure.CompanyMedia;

public sealed class DatabaseCompanyMediaStore(BillingManagementDbContext context) : ICompanyMediaStore
{
    public async Task<CompanyMediaStoredFile> StoreAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var bytes = await ReadContentAsync(content, cancellationToken);
        var key = CompanyMediaStorageKey.Create();
        context.CompanyMedia.Add(CompanyMediaFile.Create(ParseIdentifier(key), bytes));
        await context.SaveChangesAsync(cancellationToken);

        return new CompanyMediaStoredFile(key, bytes.LongLength);
    }

    public async Task<CompanyMediaStoredFile> ReplaceAsync(
        CompanyMediaStorageKey key,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(content);

        var bytes = await ReadContentAsync(content, cancellationToken);
        var identifier = ParseIdentifier(key);
        var file = await context.CompanyMedia.SingleOrDefaultAsync(
            candidate => candidate.Id == identifier,
            cancellationToken);

        if (file is null)
        {
            context.CompanyMedia.Add(CompanyMediaFile.Create(identifier, bytes));
        }
        else
        {
            file.ReplaceContent(bytes);
        }

        await context.SaveChangesAsync(cancellationToken);
        return new CompanyMediaStoredFile(key, bytes.LongLength);
    }

    public async Task<CompanyMediaReadFile?> OpenReadAsync(
        CompanyMediaStorageKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var identifier = ParseIdentifier(key);
        var file = await context.CompanyMedia
            .AsNoTracking()
            .Where(candidate => candidate.Id == identifier)
            .Select(candidate => new { candidate.Content, candidate.Length })
            .SingleOrDefaultAsync(cancellationToken);

        if (file is null)
        {
            return null;
        }

        return new CompanyMediaReadFile(
            key,
            file.Length,
            new MemoryStream(file.Content, writable: false));
    }

    public async Task<bool> DeleteAsync(
        CompanyMediaStorageKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var identifier = ParseIdentifier(key);
        var file = await context.CompanyMedia.SingleOrDefaultAsync(
            candidate => candidate.Id == identifier,
            cancellationToken);
        if (file is null)
        {
            return false;
        }

        context.CompanyMedia.Remove(file);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Guid ParseIdentifier(CompanyMediaStorageKey key)
    {
        return Guid.ParseExact(key.Value, "N");
    }

    private static async Task<byte[]> ReadContentAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }
}
