using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles;
using SkiaSharp;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class CompanyProfileIconServiceTests
{
    [Theory]
    [InlineData(SKEncodedImageFormat.Png, "image/png")]
    [InlineData(SKEncodedImageFormat.Jpeg, "image/jpeg")]
    [InlineData(SKEncodedImageFormat.Webp, "image/webp")]
    public async Task UploadAsync_stores_a_decoded_supported_image_and_persists_server_derived_metadata(
        SKEncodedImageFormat format,
        string contentType)
    {
        var profileStore = new ProfileStore(Profile());
        var mediaStore = new MediaStore();
        var service = new CompanyProfileIconService(profileStore, mediaStore);

        var result = await service.UploadAsync(new MemoryStream(CreateImage(format)));

        Assert.True(result.IsSuccess);
        Assert.Equal(contentType, result.Value!.ContentType);
        Assert.NotNull(profileStore.Profile!.IconStorageKey);
        Assert.Equal(contentType, profileStore.Profile.IconContentType);
        Assert.Single(mediaStore.Files);
    }

    [Fact]
    public async Task UploadAsync_rejects_svg_without_changing_existing_icon()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"));
        var mediaStore = new MediaStore(previousKey, CreateImage(SKEncodedImageFormat.Png));
        var service = new CompanyProfileIconService(profileStore, mediaStore);

        var result = await service.UploadAsync(new MemoryStream("<svg/>"u8.ToArray()));

        Assert.False(result.IsSuccess);
        Assert.Equal(previousKey.Value, profileStore.Profile!.IconStorageKey);
        Assert.Equal("image/png", profileStore.Profile.IconContentType);
        Assert.True(mediaStore.Files.ContainsKey(previousKey.Value));
        Assert.Single(mediaStore.Files);
    }

    [Fact]
    public async Task UploadAsync_rejects_files_larger_than_two_megabytes_without_changing_existing_icon()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"));
        var mediaStore = new MediaStore(previousKey, CreateImage(SKEncodedImageFormat.Png));
        var service = new CompanyProfileIconService(profileStore, mediaStore);
        var oversizedContent = new byte[(2 * 1024 * 1024) + 1];

        var result = await service.UploadAsync(new MemoryStream(oversizedContent));

        Assert.False(result.IsSuccess);
        Assert.Equal(previousKey.Value, profileStore.Profile!.IconStorageKey);
        Assert.True(mediaStore.Files.ContainsKey(previousKey.Value));
        Assert.Single(mediaStore.Files);
    }

    [Fact]
    public async Task UploadAsync_replaces_the_previous_icon_after_persisting_new_metadata()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"));
        var mediaStore = new MediaStore(previousKey, CreateImage(SKEncodedImageFormat.Png));
        var service = new CompanyProfileIconService(profileStore, mediaStore);

        var result = await service.UploadAsync(new MemoryStream(CreateImage(SKEncodedImageFormat.Jpeg)));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(previousKey.Value, profileStore.Profile!.IconStorageKey);
        Assert.Equal("image/jpeg", profileStore.Profile.IconContentType);
        Assert.False(mediaStore.Files.ContainsKey(previousKey.Value));
        Assert.Single(mediaStore.Files);
    }

    [Fact]
    public async Task UploadAsync_preserves_previous_icon_when_storage_or_metadata_persistence_fails()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"), updateSucceeds: false);
        var mediaStore = new MediaStore(previousKey, CreateImage(SKEncodedImageFormat.Png));
        var service = new CompanyProfileIconService(profileStore, mediaStore);

        var result = await service.UploadAsync(new MemoryStream(CreateImage(SKEncodedImageFormat.Webp)));

        Assert.False(result.IsSuccess);
        Assert.Equal(previousKey.Value, profileStore.Profile!.IconStorageKey);
        Assert.Equal("image/png", profileStore.Profile.IconContentType);
        Assert.Single(mediaStore.Files);
        Assert.True(mediaStore.Files.ContainsKey(previousKey.Value));
    }

    [Fact]
    public async Task UploadAsync_keeps_persisted_replacement_when_superseded_icon_cleanup_fails()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"));
        var mediaStore = new MediaStore(previousKey, CreateImage(SKEncodedImageFormat.Png))
        {
            DeleteExceptionKey = previousKey.Value
        };
        var service = new CompanyProfileIconService(profileStore, mediaStore);

        var result = await service.UploadAsync(new MemoryStream(CreateImage(SKEncodedImageFormat.Jpeg)));

        Assert.False(result.IsSuccess);
        var currentKey = Assert.IsType<string>(profileStore.Profile!.IconStorageKey);
        Assert.NotEqual(previousKey.Value, currentKey);
        Assert.True(mediaStore.Files.ContainsKey(currentKey));
    }

    [Fact]
    public async Task ResetAsync_persists_fallback_metadata_before_deleting_previous_icon()
    {
        var previousKey = CompanyMediaStorageKey.Create();
        var profileStore = new ProfileStore(Profile(previousKey.Value, "image/png"));
        var mediaStore = new MediaStore(previousKey, CreateImage(SKEncodedImageFormat.Png));
        var service = new CompanyProfileIconService(profileStore, mediaStore);

        var result = await service.ResetAsync();

        Assert.True(result.IsSuccess);
        Assert.Null(profileStore.Profile!.IconStorageKey);
        Assert.Null(profileStore.Profile.IconContentType);
        Assert.Empty(mediaStore.Files);
    }

    private static OwnerCompanyProfileRecord Profile(
        string? iconStorageKey = null,
        string? iconContentType = null) =>
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
            IconStorageKey: iconStorageKey,
            IconContentType: iconContentType);

    private static byte[] CreateImage(SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(1, 1);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 100);
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

        public string? DeleteExceptionKey { get; init; }

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

        public Task<bool> DeleteAsync(CompanyMediaStorageKey key, CancellationToken cancellationToken = default)
        {
            if (key.Value == this.DeleteExceptionKey)
            {
                throw new IOException("Deletion failed.");
            }

            return Task.FromResult(this.Files.Remove(key.Value));
        }
    }
}
