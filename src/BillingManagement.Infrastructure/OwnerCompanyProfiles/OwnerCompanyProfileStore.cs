using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileStore(BillingManagementDbContext dbContext)
    : IOwnerCompanyProfileStore
{
    public async Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default)
    {
        OwnerCompanyProfile? profile = await dbContext.OwnerCompanyProfiles
            .AsNoTracking()
            .OrderBy(ownerProfile => ownerProfile.CompanyName)
            .FirstOrDefaultAsync(cancellationToken);

        return profile is null ? null : ToRecord(profile);
    }

    public async Task Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
    {
        dbContext.OwnerCompanyProfiles.Add(OwnerCompanyProfile.Create(
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

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static OwnerCompanyProfileRecord ToRecord(OwnerCompanyProfile profile) =>
        new(
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
            profile.RegistrationNumber);
}
