using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileStore(BillingManagementDbContext dbContext)
    : IOwnerCompanyProfileStore
{
    public async Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.OwnerCompanyProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        return profile is null ? null : ToRecord(profile);
    }

    public async Task<bool> Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
    {
        dbContext.OwnerCompanyProfiles.Add(OwnerCompanyProfile.Rehydrate(
            profile.Id,
            profile.CompanyName,
            profile.AddressLine1,
            profile.AddressLine2,
            profile.City,
            profile.PostalCode,
            profile.Country,
            profile.TaxId,
            profile.Phone,
            profile.Email,
            profile.Website,
            profile.LogoReference,
            profile.RegistrationNumber));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            dbContext.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> Update(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
    {
        var existingProfile = await dbContext.OwnerCompanyProfiles
            .SingleOrDefaultAsync(cancellationToken);

        if (existingProfile is null)
        {
            return false;
        }

        existingProfile.Update(
            profile.CompanyName,
            profile.AddressLine1,
            profile.AddressLine2,
            profile.City,
            profile.PostalCode,
            profile.Country,
            profile.TaxId,
            profile.Phone,
            profile.Email,
            profile.Website,
            profile.LogoReference,
            profile.RegistrationNumber);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<OwnerCompanyProfileDeleteResult> Delete(
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.OwnerCompanyProfiles.SingleOrDefaultAsync(cancellationToken);
        if (profile is null)
        {
            return OwnerCompanyProfileDeleteResult.NotFound;
        }

        dbContext.OwnerCompanyProfiles.Remove(profile);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OwnerCompanyProfileDeleteResult.Deleted;
    }

    private static OwnerCompanyProfileRecord ToRecord(OwnerCompanyProfile profile) =>
        new(
            profile.Id,
            profile.CompanyName,
            profile.AddressLine1,
            profile.AddressLine2,
            profile.CityProvinceState,
            profile.PostalCode,
            profile.Country,
            profile.TaxId,
            profile.Phone,
            profile.Email,
            profile.Website,
            profile.LogoReference,
            profile.RegistrationNumber);
}
