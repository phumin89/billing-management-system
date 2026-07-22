using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles;
using SkiaSharp;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class CompanyProfileCoverServiceTests
{
    private static readonly byte[] ValidPng = CreatePng();

    [Fact]
    public async Task UploadAsync_stores_a_decoded_png_and_persists_server_derived_metadata()
    {
        var profileStore = new ProfileStore(Profile());
        var mediaStore = new MediaStore();
        var service = new CompanyProfileCoverService(profileStore, mediaStore);

        var result = await service.UploadAsync(new MemoryStream(ValidPng));

        Assert.True(result.IsSuccess);
        Assert.Equal("image/png", result.Value!.ContentType);
        Assert.NotNull(profileStore.Profile!.CoverStorageKey);
        Assert.Equal("image/png", profileStore.Profile.CoverContentType);
        Assert.Single(mediaStore.Files);
    }

    [Fact]
    public async Task UploadAsync_rejects_svg_without_changing_existing_cover()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"));
        var mediaStore = new MediaStore(previousKey, ValidPng);
        var service = new CompanyProfileCoverService(profileStore, mediaStore);

        var result = await service.UploadAsync(new MemoryStream("<svg/>"u8.ToArray()));

        Assert.False(result.IsSuccess);
        Assert.Equal(previousKey.Value, profileStore.Profile!.CoverStorageKey);
        Assert.Equal("image/png", profileStore.Profile.CoverContentType);
        Assert.True(mediaStore.Files.ContainsKey(previousKey.Value));
        Assert.Single(mediaStore.Files);
    }

    [Fact]
    public async Task UploadAsync_preserves_previous_cover_when_metadata_persistence_fails()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"), updateSucceeds: false);
        var mediaStore = new MediaStore(previousKey, ValidPng);
        var service = new CompanyProfileCoverService(profileStore, mediaStore);

        var result = await service.UploadAsync(new MemoryStream(ValidPng));

        Assert.False(result.IsSuccess);
        Assert.Equal(previousKey.Value, profileStore.Profile!.CoverStorageKey);
        Assert.Single(mediaStore.Files);
        Assert.True(mediaStore.Files.ContainsKey(previousKey.Value));
    }

    [Fact]
    public async Task ResetAsync_persists_default_metadata_before_deleting_previous_cover()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"));
        var mediaStore = new MediaStore(previousKey, ValidPng);
        var service = new CompanyProfileCoverService(profileStore, mediaStore);

        var result = await service.ResetAsync();

        Assert.True(result.IsSuccess);
        Assert.Null(profileStore.Profile!.CoverStorageKey);
        Assert.Null(profileStore.Profile.CoverContentType);
        Assert.Empty(mediaStore.Files);
    }

    private static OwnerCompanyProfileRecord Profile(
        string? coverStorageKey = null,
        string? coverContentType = null) =>
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
            coverStorageKey,
            coverContentType);

    private static byte[] CreatePng()
    {
        using var bitmap = new SKBitmap(1, 1);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private sealed class ProfileStore(OwnerCompanyProfileRecord profile, bool updateSucceeds = true)
        : IOwnerCompanyProfileStore
    {
        public OwnerCompanyProfileRecord? Profile { get; private set; } = profile;

        public Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(this.Profile);

        public Task<bool> Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> Update(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
        {
            if (!updateSucceeds)
            {
                return Task.FromResult(false);
            }

            this.Profile = profile;
            return Task.FromResult(true);
        }

        public Task<OwnerCompanyProfileDeleteResult> Delete(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class MediaStore : ICompanyMediaStore
    {
        public MediaStore()
        {
        }

        public MediaStore(CompanyMediaStorageKey key, byte[] content)
        {
            this.Files.Add(key.Value, content);
        }

        public Dictionary<string, byte[]> Files { get; } = [];

        public async Task<CompanyMediaStoredFile> StoreAsync(Stream content, CancellationToken cancellationToken = default)
        {
            await using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            var key = CompanyMediaStorageKey.Create();
            this.Files.Add(key.Value, buffer.ToArray());
            return new CompanyMediaStoredFile(key, buffer.Length);
        }

        public async Task<CompanyMediaStoredFile> ReplaceAsync(
            CompanyMediaStorageKey key,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            await using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            this.Files[key.Value] = buffer.ToArray();
            return new CompanyMediaStoredFile(key, buffer.Length);
        }

        public Task<CompanyMediaReadFile?> OpenReadAsync(
            CompanyMediaStorageKey key,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(this.Files.TryGetValue(key.Value, out var content)
                ? new CompanyMediaReadFile(key, content.Length, new MemoryStream(content))
                : null);

        public Task<bool> DeleteAsync(CompanyMediaStorageKey key, CancellationToken cancellationToken = default) =>
            Task.FromResult(this.Files.Remove(key.Value));
    }
}
