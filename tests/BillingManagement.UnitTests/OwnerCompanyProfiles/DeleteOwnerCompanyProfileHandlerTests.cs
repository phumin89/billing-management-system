using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.OwnerCompanyProfiles.DeleteOwnerCompanyProfile;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class DeleteOwnerCompanyProfileHandlerTests
{
    [Fact]
    public async Task Handle_returns_success_when_profile_deleted()
    {
        var handler = new DeleteOwnerCompanyProfileHandler(
            new StubStore(OwnerCompanyProfileDeleteResult.Deleted));

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task Handle_returns_not_found_when_profile_missing()
    {
        var handler = new DeleteOwnerCompanyProfileHandler(
            new StubStore(OwnerCompanyProfileDeleteResult.NotFound));

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ApplicationErrorKind.NotFound, result.Error.Kind);
        Assert.Equal("owner_company_profile.not_found", result.Error.Code);
        Assert.Equal("Owner company profile was not found.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_returns_conflict_when_profile_has_document_dependencies()
    {
        var handler = new DeleteOwnerCompanyProfileHandler(
            new StubStore(OwnerCompanyProfileDeleteResult.DependencyConflict));

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ApplicationErrorKind.Conflict, result.Error.Kind);
        Assert.Equal("owner_company_profile.in_use", result.Error.Code);
        Assert.Equal(
            "Company profile is used by quotations or invoices and cannot be deleted.",
            result.Error.Message);
    }

    private sealed class StubStore(OwnerCompanyProfileDeleteResult deleteResult)
        : IOwnerCompanyProfileStore
    {
        public Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<OwnerCompanyProfileRecord?>(null);

        public Task<bool> Add(
            OwnerCompanyProfileRecord profile,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> Update(
            OwnerCompanyProfileRecord profile,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<OwnerCompanyProfileDeleteResult> Delete(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(deleteResult);
    }
}
