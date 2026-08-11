using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;

public sealed class GetOwnerCompanyProfileHandler(
    IOwnerCompanyProfileStore store)
    : IQueryHandler<GetOwnerCompanyProfileQuery, GetOwnerCompanyProfileResult>
{
    public async ValueTask<GetOwnerCompanyProfileResult> Handle(
        GetOwnerCompanyProfileQuery query,
        CancellationToken cancellationToken = default) =>
        new(await store.GetAsync(cancellationToken));
}
