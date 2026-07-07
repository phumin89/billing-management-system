using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;

public sealed class GetOwnerCompanyProfileHandler(
    IOwnerCompanyProfileStore store)
    : IQueryHandler<GetOwnerCompanyProfileQuery, OwnerCompanyProfileRecord?>
{
    public Task<OwnerCompanyProfileRecord?> Handle(
        GetOwnerCompanyProfileQuery query,
        CancellationToken cancellationToken = default) =>
        store.GetAsync(cancellationToken);
}
