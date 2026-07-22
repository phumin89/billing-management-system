using System.Text;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Infrastructure.CompanyMedia;

namespace BillingManagement.UnitTests.CompanyMedia;

public sealed class FileSystemCompanyMediaStoreTests : IDisposable
{
    private readonly string rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task StoreAsync_generates_an_opaque_key_beneath_the_configured_root()
    {
        var store = this.CreateStore();

        var stored = await store.StoreAsync(Content("company logo"));

        Assert.Matches("^[a-f0-9]{32}$", stored.Key.Value);
        Assert.Equal(12, stored.Length);
        Assert.True(File.Exists(Path.Combine(this.rootPath, stored.Key.Value)));
    }

    [Fact]
    public async Task OpenReadAsync_returns_content_after_store_recreation()
    {
        var stored = await this.CreateStore().StoreAsync(Content("persistent content"));

        var file = await this.CreateStore().OpenReadAsync(stored.Key);

        Assert.NotNull(file);
        Assert.Equal(stored.Key, file.Key);
        Assert.Equal(stored.Length, file.Length);
        await using var content = file.Content;
        Assert.Equal("persistent content", await new StreamReader(content).ReadToEndAsync());
    }

    [Fact]
    public async Task OpenReadAsync_and_DeleteAsync_return_expected_outcomes_for_missing_or_deleted_files()
    {
        var store = this.CreateStore();
        var missingKey = CompanyMediaStorageKey.Create();
        var stored = await store.StoreAsync(Content("delete me"));

        Assert.Null(await store.OpenReadAsync(missingKey));
        Assert.False(await store.DeleteAsync(missingKey));
        Assert.True(await store.DeleteAsync(stored.Key));
        Assert.Null(await store.OpenReadAsync(stored.Key));
        Assert.False(await store.DeleteAsync(stored.Key));
    }

    [Fact]
    public async Task ReplaceAsync_retains_existing_content_when_the_new_write_fails()
    {
        var store = this.CreateStore();
        var stored = await store.StoreAsync(Content("original content"));

        await Assert.ThrowsAsync<IOException>(() => store.ReplaceAsync(stored.Key, new ThrowingReadStream()));

        var file = await store.OpenReadAsync(stored.Key);
        Assert.NotNull(file);
        await using var content = file.Content;
        Assert.Equal("original content", await new StreamReader(content).ReadToEndAsync());
    }

    public void Dispose()
    {
        if (Directory.Exists(this.rootPath))
        {
            Directory.Delete(this.rootPath, recursive: true);
        }
    }

    private FileSystemCompanyMediaStore CreateStore() =>
        new(new CompanyMediaStorageOptions { RootPath = this.rootPath });

    private static MemoryStream Content(string value) =>
        new(Encoding.UTF8.GetBytes(value));

    private sealed class ThrowingReadStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => 0;

        public override long Position { get => 0; set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new IOException("The content stream failed.");

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
