using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using SkiaSharp;

namespace BillingManagement.Application.OwnerCompanyProfiles;

public sealed class CompanyProfileIconService : ICompanyProfileIconService
{
    private const long MaximumLength = 2 * 1024 * 1024;
    private const long MaximumPixels = 25_000_000;
    private readonly IOwnerCompanyProfileStore profileStore;
    private readonly ICompanyMediaStore mediaStore;

    public CompanyProfileIconService(
        IOwnerCompanyProfileStore profileStore,
        ICompanyMediaStore mediaStore)
    {
        this.profileStore = profileStore;
        this.mediaStore = mediaStore;
    }

    public async Task<ApplicationResult<CompanyProfileIconDescriptor>> UploadAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var existingProfile = await this.profileStore.GetAsync(cancellationToken);
        if (existingProfile is null)
        {
            return MissingProfile<CompanyProfileIconDescriptor>();
        }

        var imageContent = await ReadValidatedContentAsync(content, cancellationToken);
        if (!imageContent.IsSuccess)
        {
            return ApplicationResult<CompanyProfileIconDescriptor>.Failure(imageContent.Error!);
        }

        CompanyMediaStoredFile? storedFile = null;
        var metadataPersisted = false;
        try
        {
            await using var storedContent = new MemoryStream(imageContent.Value!.Content, writable: false);
            storedFile = await this.mediaStore.StoreAsync(storedContent, cancellationToken);

            var updatedProfile = existingProfile with
            {
                IconStorageKey = storedFile.Key.Value,
                IconContentType = imageContent.Value.ContentType
            };
            if (!await this.profileStore.Update(updatedProfile, cancellationToken))
            {
                await this.mediaStore.DeleteAsync(storedFile.Key, cancellationToken);
                return MissingProfile<CompanyProfileIconDescriptor>();
            }

            metadataPersisted = true;
            await this.DeleteSupersededIconAsync(existingProfile.IconStorageKey, cancellationToken);
            return ApplicationResult<CompanyProfileIconDescriptor>.Success(
                new CompanyProfileIconDescriptor(imageContent.Value.ContentType, storedFile.Length));
        }
        catch
        {
            if (storedFile is not null && !metadataPersisted)
            {
                await this.mediaStore.DeleteAsync(storedFile.Key, cancellationToken);
            }

            return ApplicationResult<CompanyProfileIconDescriptor>.Failure(ApplicationError.Failure(
                "owner_company_profile.icon_upload_failed",
                "Company profile icon could not be saved."));
        }
    }

    public async Task<ApplicationResult<CompanyProfileIconFile>> OpenReadAsync(
        CancellationToken cancellationToken = default)
    {
        var profile = await this.profileStore.GetAsync(cancellationToken);
        if (profile?.IconStorageKey is null || profile.IconContentType is null)
        {
            return MissingProfile<CompanyProfileIconFile>();
        }

        var file = await this.mediaStore.OpenReadAsync(
            CompanyMediaStorageKey.Parse(profile.IconStorageKey),
            cancellationToken);
        if (file is null)
        {
            return MissingProfile<CompanyProfileIconFile>();
        }

        return ApplicationResult<CompanyProfileIconFile>.Success(
            new CompanyProfileIconFile(profile.IconContentType, file.Length, file.Content));
    }

    public async Task<ApplicationResult<bool>> ResetAsync(CancellationToken cancellationToken = default)
    {
        var existingProfile = await this.profileStore.GetAsync(cancellationToken);
        if (existingProfile is null)
        {
            return MissingProfile<bool>();
        }

        if (!await this.profileStore.Update(existingProfile with { IconStorageKey = null, IconContentType = null }, cancellationToken))
        {
            return MissingProfile<bool>();
        }

        await this.DeleteSupersededIconAsync(existingProfile.IconStorageKey, cancellationToken);
        return ApplicationResult<bool>.Success(true);
    }

    private static async Task<ApplicationResult<ValidatedIconContent>> ReadValidatedContentAsync(
        Stream content,
        CancellationToken cancellationToken)
    {
        byte[] bytes;
        try
        {
            bytes = await ReadAtMostAsync(content, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return InvalidFile<ValidatedIconContent>(exception.Message);
        }

        await using var imageContent = new MemoryStream(bytes, writable: false);
        using var codec = SKCodec.Create(imageContent);
        if (codec is null)
        {
            return InvalidFile<ValidatedIconContent>("Only PNG, JPEG, and WebP images are supported.");
        }

        if ((long)codec.Info.Width * codec.Info.Height > MaximumPixels)
        {
            return InvalidFile<ValidatedIconContent>("Image dimensions cannot exceed 25 megapixels.");
        }

        var contentType = codec.EncodedFormat switch
        {
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Webp => "image/webp",
            _ => null
        };
        if (contentType is null)
        {
            return InvalidFile<ValidatedIconContent>("Only PNG, JPEG, and WebP images are supported.");
        }

        using var bitmap = new SKBitmap(codec.Info);
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
        {
            return InvalidFile<ValidatedIconContent>("The uploaded image is malformed.");
        }

        return ApplicationResult<ValidatedIconContent>.Success(new ValidatedIconContent(bytes, contentType));
    }

    private static async Task<byte[]> ReadAtMostAsync(Stream content, CancellationToken cancellationToken)
    {
        await using var buffer = new MemoryStream();
        var chunk = new byte[81920];

        while (true)
        {
            var read = await content.ReadAsync(chunk, cancellationToken);
            if (read == 0)
            {
                return buffer.ToArray();
            }

            if (buffer.Length + read > MaximumLength)
            {
                throw new InvalidOperationException("Image files cannot exceed 2 MB.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
    }

    private async Task DeleteSupersededIconAsync(string? storageKey, CancellationToken cancellationToken)
    {
        if (storageKey is not null)
        {
            await this.mediaStore.DeleteAsync(CompanyMediaStorageKey.Parse(storageKey), cancellationToken);
        }
    }

    private static ApplicationResult<T> InvalidFile<T>(string message) =>
        ApplicationResult<T>.Failure(ApplicationError.Validation(
            "owner_company_profile.icon_invalid",
            "The company profile icon is invalid.",
            new Dictionary<string, string[]> { ["file"] = [message] }));

    private static ApplicationResult<T> MissingProfile<T>() =>
        ApplicationResult<T>.Failure(ApplicationError.NotFound(
            "owner_company_profile.icon_not_found",
            "Company profile icon was not found."));

    private sealed record ValidatedIconContent(byte[] Content, string ContentType);
}
