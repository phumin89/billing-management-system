using BillingManagement.Application.Abstractions.CompanyMedia;

namespace BillingManagement.Infrastructure.CompanyMedia;

public sealed class FileSystemCompanyMediaStore : ICompanyMediaStore
{
    private readonly string rootPath;

    public FileSystemCompanyMediaStore(CompanyMediaStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.RootPath))
        {
            throw new ArgumentException("Company media storage root path is required.", nameof(options));
        }

        this.rootPath = Path.GetFullPath(options.RootPath);
    }

    public async Task<CompanyMediaStoredFile> StoreAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        Directory.CreateDirectory(this.rootPath);

        while (true)
        {
            var key = CompanyMediaStorageKey.Create();
            var destinationPath = this.GetFilePath(key);

            if (File.Exists(destinationPath))
            {
                continue;
            }

            var length = await this.WriteAtomicallyAsync(content, destinationPath, overwrite: false, cancellationToken);
            return new CompanyMediaStoredFile(key, length);
        }
    }

    public async Task<CompanyMediaStoredFile> ReplaceAsync(
        CompanyMediaStorageKey key,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(content);

        Directory.CreateDirectory(this.rootPath);
        var length = await this.WriteAtomicallyAsync(
            content,
            this.GetFilePath(key),
            overwrite: true,
            cancellationToken);

        return new CompanyMediaStoredFile(key, length);
    }

    public Task<CompanyMediaReadFile?> OpenReadAsync(
        CompanyMediaStorageKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();

        var path = this.GetFilePath(key);
        if (!File.Exists(path))
        {
            return Task.FromResult<CompanyMediaReadFile?>(null);
        }

        var content = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult<CompanyMediaReadFile?>(
            new CompanyMediaReadFile(key, content.Length, content));
    }

    public Task<bool> DeleteAsync(
        CompanyMediaStorageKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();

        var path = this.GetFilePath(key);
        if (!File.Exists(path))
        {
            return Task.FromResult(false);
        }

        File.Delete(path);
        return Task.FromResult(true);
    }

    private string GetFilePath(CompanyMediaStorageKey key)
    {
        var path = Path.GetFullPath(Path.Combine(this.rootPath, key.Value));
        var rootPrefix = this.rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? this.rootPath
            : this.rootPath + Path.DirectorySeparatorChar;

        if (!path.StartsWith(rootPrefix, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Company media storage key resolves outside the configured root.");
        }

        return path;
    }

    private async Task<long> WriteAtomicallyAsync(
        Stream content,
        string destinationPath,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var temporaryPath = Path.Combine(this.rootPath, $".{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var destination = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous))
            {
                await content.CopyToAsync(destination, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, destinationPath, overwrite);
            return new FileInfo(destinationPath).Length;
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            throw;
        }
    }
}
