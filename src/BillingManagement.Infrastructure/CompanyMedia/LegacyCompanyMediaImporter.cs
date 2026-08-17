using BillingManagement.Application.Abstractions.CompanyMedia;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure.CompanyMedia;

public sealed class LegacyCompanyMediaImporter(BillingManagementDbContext context)
{
    public async Task<int> ImportAsync(
        string? rootPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
        {
            return 0;
        }

        var keys = await this.GetReferencedKeysAsync(cancellationToken);
        var imported = 0;

        foreach (var key in keys)
        {
            var identifier = Guid.ParseExact(key.Value, "N");
            if (await context.CompanyMedia.AnyAsync(file => file.Id == identifier, cancellationToken))
            {
                continue;
            }

            var path = Path.Combine(rootPath, key.Value);
            if (!File.Exists(path))
            {
                continue;
            }

            var content = await File.ReadAllBytesAsync(path, cancellationToken);
            context.CompanyMedia.Add(CompanyMediaFile.Create(identifier, content));
            imported++;
        }

        if (imported > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return imported;
    }

    private async Task<IReadOnlyList<CompanyMediaStorageKey>> GetReferencedKeysAsync(
        CancellationToken cancellationToken)
    {
        var profile = await context.OwnerCompanyProfiles
            .AsNoTracking()
            .Select(owner => new { owner.CoverStorageKey, owner.IconStorageKey })
            .SingleOrDefaultAsync(cancellationToken);
        if (profile is null)
        {
            return [];
        }

        return new[] { profile.CoverStorageKey, profile.IconStorageKey }
            .Where(key => key is not null)
            .Distinct(StringComparer.Ordinal)
            .Select(key => CompanyMediaStorageKey.Parse(key!))
            .ToArray();
    }
}
