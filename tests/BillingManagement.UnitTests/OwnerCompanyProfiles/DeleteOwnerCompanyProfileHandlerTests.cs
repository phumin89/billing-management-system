using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.OwnerCompanyProfiles.DeleteOwnerCompanyProfile;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class DeleteOwnerCompanyProfileHandlerTests
{
    [Fact]
    public async Task Handle_deletes_stored_cover_and_icon_after_profile_is_deleted()
    {
        var coverKey = Guid.NewGuid().ToString("N");
        var iconKey = Guid.NewGuid().ToString("N");
        var mediaStore = new StubMediaStore();
        var handler = new DeleteOwnerCompanyProfileHandler(
            new StubStore(
                OwnerCompanyProfileDeleteResult.Deleted,
                Profile(coverKey, iconKey)),
            mediaStore,
            NullLogger<DeleteOwnerCompanyProfileHandler>.Instance);

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal([coverKey, iconKey], mediaStore.DeletedKeys);
    }

    [Fact]
    public async Task Handle_succeeds_when_stored_media_files_are_already_missing()
    {
        var coverKey = Guid.NewGuid().ToString("N");
        var iconKey = Guid.NewGuid().ToString("N");
        var mediaStore = new StubMediaStore { DeleteResult = false };
        var handler = this.CreateHandler(
            new StubStore(OwnerCompanyProfileDeleteResult.Deleted, Profile(coverKey, iconKey)),
            mediaStore);

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal([coverKey, iconKey], mediaStore.DeletedKeys);
    }

    [Fact]
    public async Task Handle_succeeds_without_media_when_profile_is_deleted()
    {
        var mediaStore = new StubMediaStore();
        var handler = this.CreateHandler(
            new StubStore(OwnerCompanyProfileDeleteResult.Deleted, Profile()),
            mediaStore);

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.True(result.IsSuccess);
        Assert.Empty(mediaStore.DeletedKeys);
    }

    [Theory]
    [InlineData(OwnerCompanyProfileDeleteResult.NotFound)]
    [InlineData(OwnerCompanyProfileDeleteResult.DependencyConflict)]
    public async Task Handle_does_not_delete_media_when_profile_is_not_deleted(
        OwnerCompanyProfileDeleteResult deleteResult)
    {
        var mediaStore = new StubMediaStore();
        var handler = this.CreateHandler(
            new StubStore(deleteResult, Profile(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"))),
            mediaStore);

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.False(result.IsSuccess);
        Assert.Empty(mediaStore.DeletedKeys);
    }

    [Fact]
    public async Task Handle_logs_warning_and_succeeds_when_media_cleanup_fails()
    {
        var coverKey = Guid.NewGuid().ToString("N");
        var iconKey = Guid.NewGuid().ToString("N");
        var mediaStore = new StubMediaStore { ThrowOnDeleteKey = coverKey };
        var logger = new RecordingLogger();
        var handler = this.CreateHandler(
            new StubStore(OwnerCompanyProfileDeleteResult.Deleted, Profile(coverKey, iconKey)),
            mediaStore,
            logger);

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal([coverKey, iconKey], mediaStore.DeletedKeys);
        Assert.Equal(LogLevel.Warning, Assert.Single(logger.LogLevels));
    }

    [Fact]
    public async Task Handle_returns_success_when_profile_deleted()
    {
        var handler = this.CreateHandler(new StubStore(OwnerCompanyProfileDeleteResult.Deleted));

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task Handle_returns_not_found_when_profile_missing()
    {
        var handler = this.CreateHandler(new StubStore(OwnerCompanyProfileDeleteResult.NotFound));

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
        var handler = this.CreateHandler(new StubStore(OwnerCompanyProfileDeleteResult.DependencyConflict));

        var result = await handler.Handle(new DeleteOwnerCompanyProfileCommand());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ApplicationErrorKind.Conflict, result.Error.Kind);
        Assert.Equal("owner_company_profile.in_use", result.Error.Code);
        Assert.Equal(
            "Company profile is used by quotations or invoices and cannot be deleted.",
            result.Error.Message);
    }

    private static OwnerCompanyProfileRecord Profile(string? coverKey = null, string? iconKey = null) =>
        new(
            Guid.NewGuid(),
            "Acme Co.",
            "1 Main Street",
            null,
            "Bangkok",
            "10110",
            "Thailand",
            null,
            null,
            null,
            null,
            null,
            null,
            coverKey,
            coverKey is null ? null : "image/png",
            iconKey,
            iconKey is null ? null : "image/png");

    private DeleteOwnerCompanyProfileHandler CreateHandler(
        IOwnerCompanyProfileStore store,
        ICompanyMediaStore? mediaStore = null,
        ILogger<DeleteOwnerCompanyProfileHandler>? logger = null) =>
        new(
            store,
            mediaStore ?? new StubMediaStore(),
            logger ?? NullLogger<DeleteOwnerCompanyProfileHandler>.Instance);

    private sealed class StubStore(
        OwnerCompanyProfileDeleteResult deleteResult,
        OwnerCompanyProfileRecord? profile = null)
        : IOwnerCompanyProfileStore
    {
        public Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(profile);

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

    private sealed class StubMediaStore : ICompanyMediaStore
    {
        public List<string> DeletedKeys { get; } = [];

        public bool DeleteResult { get; init; } = true;

        public string? ThrowOnDeleteKey { get; init; }

        public Task<CompanyMediaStoredFile> StoreAsync(
            Stream content,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<CompanyMediaStoredFile> ReplaceAsync(
            CompanyMediaStorageKey key,
            Stream content,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<CompanyMediaReadFile?> OpenReadAsync(
            CompanyMediaStorageKey key,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> DeleteAsync(
            CompanyMediaStorageKey key,
            CancellationToken cancellationToken = default)
        {
            this.DeletedKeys.Add(key.Value);
            if (key.Value == this.ThrowOnDeleteKey)
            {
                throw new InvalidOperationException("The media store is unavailable.");
            }

            return Task.FromResult(this.DeleteResult);
        }
    }

    private sealed class RecordingLogger : ILogger<DeleteOwnerCompanyProfileHandler>
    {
        public List<LogLevel> LogLevels { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            this.LogLevels.Add(logLevel);
        }
    }
}
