using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;

public sealed record GetOwnerCompanyProfileResult(OwnerCompanyProfileRecord? Profile) : IQueryResult;
