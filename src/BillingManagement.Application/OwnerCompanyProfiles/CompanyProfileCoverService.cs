using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using SkiaSharp;

namespace BillingManagement.Application.OwnerCompanyProfiles;

public sealed class CompanyProfileCoverService : ICompanyProfileCoverService
{
    private const long MaximumLength = 5 * 1024 * 1024;
    private const long MaximumPixels = 25_000_000;
    private readonly IOwnerCompanyProfileStore profileStore;
    private readonly ICompanyMediaStore mediaStore;

    public CompanyProfileCoverService(
        IOwnerCompanyProfileStore profileStore,
        ICompanyMediaStore mediaStore)
    {
        this.profileStore = profileStore;
        this.mediaStore = mediaStore;
    }

    public async Task<ApplicationResult<CompanyProfileCoverDescriptor>> UploadAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var existingProfile = await this.profileStore.GetAsync(cancellationToken);
        if (existingProfile is null)
        {
            return MissingProfile<CompanyProfileCoverDescriptor>();
        }

        var imageContent = await ReadValidatedContentAsync(content, cancellationToken);
        if (!imageContent.IsSuccess)
        {
            return ApplicationResult<CompanyProfileCoverDescriptor>.Failure(imageContent.Error!);
        }

        CompanyMediaStoredFile? storedFile = null;
        try
        {
            await using var storedContent = new MemoryStream(imageContent.Value!.Content, writable: false);
            storedFile = await this.mediaStore.StoreAsync(storedContent, cancellationToken);

            var updatedProfile = existingProfile with
            {
                CoverStorageKey = storedFile.Key.Value,
                CoverContentType = imageContent.Value.ContentType
            };
            if (!await this.profileStore.Update(updatedProfile, cancellationToken))
            {
                await this.mediaStore.DeleteAsync(storedFile.Key, cancellationToken);
                return MissingProfile<CompanyProfileCoverDescriptor>();
            }

            await this.DeleteSupersededCoverAsync(existingProfile.CoverStorageKey, cancellationToken);
            return ApplicationResult<CompanyProfileCoverDescriptor>.Success(
                new CompanyProfileCoverDescriptor(imageContent.Value.ContentType, storedFile.Length));
        }
        catch
        {
            if (storedFile is not null)
            {
                await this.mediaStore.DeleteAsync(storedFile.Key, cancellationToken);
            }

            return ApplicationResult<CompanyProfileCoverDescriptor>.Failure(ApplicationError.Failure(
                "owner_company_profile.cover_upload_failed",
                "Company profile cover could not be saved."));
        }
    }

    public async Task<ApplicationResult<CompanyProfileCoverFile>> OpenReadAsync(
        CancellationToken cancellationToken = default)
    {
        var profile = await this.profileStore.GetAsync(cancellationToken);
        if (profile?.CoverStorageKey is null || profile.CoverContentType is null)
        {
            return MissingProfile<CompanyProfileCoverFile>();
        }

        var file = await this.mediaStore.OpenReadAsync(
            CompanyMediaStorageKey.Parse(profile.CoverStorageKey),
            cancellationToken);
        if (file is null)
        {
            return MissingProfile<CompanyProfileCoverFile>();
        }

        return ApplicationResult<CompanyProfileCoverFile>.Success(
            new CompanyProfileCoverFile(profile.CoverContentType, file.Length, file.Content));
    }

    public async Task<ApplicationResult<bool>> ResetAsync(CancellationToken cancellationToken = default)
    {
        var existingProfile = await this.profileStore.GetAsync(cancellationToken);
        if (existingProfile is null)
        {
            return MissingProfile<bool>();
        }

        if (!await this.profileStore.Update(existingProfile with { CoverStorageKey = null, CoverContentType = null }, cancellationToken))
        {
            return MissingProfile<bool>();
        }

        await this.DeleteSupersededCoverAsync(existingProfile.CoverStorageKey, cancellationToken);
        return ApplicationResult<bool>.Success(true);
    }

    private static async Task<ApplicationResult<ValidatedCoverContent>> ReadValidatedContentAsync(
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
            return InvalidFile<ValidatedCoverContent>(exception.Message);
        }

        await using var imageContent = new MemoryStream(bytes, writable: false);
        using var codec = SKCodec.Create(imageContent);
        if (codec is null)
        {
            return InvalidFile<ValidatedCoverContent>("Only PNG, JPEG, and WebP images are supported.");
        }

        if ((long)codec.Info.Width * codec.Info.Height > MaximumPixels)
        {
            return InvalidFile<ValidatedCoverContent>("Image dimensions cannot exceed 25 megapixels.");
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
            return InvalidFile<ValidatedCoverContent>("Only PNG, JPEG, and WebP images are supported.");
        }

        using var bitmap = new SKBitmap(codec.Info);
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
        {
            return InvalidFile<ValidatedCoverContent>("The uploaded image is malformed.");
        }

        return ApplicationResult<ValidatedCoverContent>.Success(new ValidatedCoverContent(bytes, contentType));
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
                throw new InvalidOperationException("Image files cannot exceed 5 MB.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
    }

    private async Task DeleteSupersededCoverAsync(string? storageKey, CancellationToken cancellationToken)
    {
        if (storageKey is not null)
        {
            await this.mediaStore.DeleteAsync(CompanyMediaStorageKey.Parse(storageKey), cancellationToken);
        }
    }

    private static ApplicationResult<T> InvalidFile<T>(string message) =>
        ApplicationResult<T>.Failure(ApplicationError.Validation(
            "owner_company_profile.cover_invalid",
            "The company profile cover is invalid.",
            new Dictionary<string, string[]> { ["file"] = [message] }));

    private static ApplicationResult<T> MissingProfile<T>() =>
        ApplicationResult<T>.Failure(ApplicationError.NotFound(
            "owner_company_profile.cover_not_found",
            "Company profile cover was not found."));

    private sealed record ValidatedCoverContent(byte[] Content, string ContentType);
}
