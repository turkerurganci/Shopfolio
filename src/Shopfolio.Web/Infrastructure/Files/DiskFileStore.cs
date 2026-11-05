using System.Runtime.CompilerServices;

namespace Shopfolio.Infrastructure.Files;

/// <summary>The directories of the two sections (05 7.7.1): the Compose volumes of the images and of the digital files (DEPLOY_RUNBOOK; deploy/compose.yaml).</summary>
internal sealed record FileStoreDirectories(string Images, string Digital)
{
    public const string DefaultImagesDirectory = "/var/lib/shopfolio/images";

    public const string DefaultDigitalDirectory = "/var/lib/shopfolio/digital";

    public static FileStoreDirectories Default { get; } = new(DefaultImagesDirectory, DefaultDigitalDirectory);

    public string Of(FileArea area) => area switch
    {
        FileArea.Image => Images,
        FileArea.DigitalFile => Digital,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown file section."),
    };
}

/// <summary>
/// The disk implementation of the file store (09 6 item 6; 05 7.7.1). The file name is the key; the content is written to a temporary file of the
/// same directory and renamed when complete, so a reader never sees a half file. A temporary file does not fit a key's shape and is never listed.
/// </summary>
internal sealed class DiskFileStore : IFileStore
{
    private const string TemporaryPrefix = ".tmp-";

    private readonly FileStoreDirectories _directories;
    private readonly TimeProvider _time;

    public DiskFileStore(FileStoreDirectories directories, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(directories);
        ArgumentNullException.ThrowIfNull(time);
        _directories = directories;
        _time = time;
    }

    public async Task WriteAsync(FileArea area, string key, Stream content, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);
        string target = PathOf(area, key);
        Directory.CreateDirectory(_directories.Of(area));
        string temporary = Path.Combine(_directories.Of(area), TemporaryPrefix + Path.GetRandomFileName());
        try
        {
            await using (FileStream file = new(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
            {
                await content.CopyToAsync(file, ct);
                await file.FlushAsync(ct);
            }

            File.Move(temporary, target, overwrite: true);
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    public async Task AppendAsync(FileArea area, string key, Stream content, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (area != FileArea.DigitalFile)
        {
            throw new ArgumentException("Only a digital file is uploaded in chunks.", nameof(area));
        }

        string target = PathOf(area, key);
        Directory.CreateDirectory(_directories.Of(area));
        await using FileStream file = new(target, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
        await content.CopyToAsync(file, ct);
        await file.FlushAsync(ct);
    }

    public Task<Stream> OpenReadAsync(FileArea area, string key, CancellationToken ct)
    {
        Stream stream = new FileStream(PathOf(area, key), FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(FileArea area, string key, CancellationToken ct) => Task.FromResult(File.Exists(PathOf(area, key)));

    public Task<long> GetSizeAsync(FileArea area, string key, CancellationToken ct)
    {
        FileInfo info = new(PathOf(area, key));
        return Task.FromResult(info.Exists ? info.Length : 0L);
    }

    public Task DeleteAsync(FileArea area, string key, CancellationToken ct)
    {
        File.Delete(PathOf(area, key));
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> ListAsync(FileArea area, TimeSpan minAge, [EnumeratorCancellation] CancellationToken ct)
    {
        string directory = _directories.Of(area);
        if (!Directory.Exists(directory))
        {
            yield break;
        }

        DateTimeOffset cutoff = _time.GetUtcNow() - minAge;
        foreach (string path in Directory.EnumerateFiles(directory))
        {
            ct.ThrowIfCancellationRequested();
            string name = Path.GetFileName(path);
            if (FileKeys.IsValid(area, name) && new DateTimeOffset(File.GetLastWriteTimeUtc(path), TimeSpan.Zero) <= cutoff)
            {
                yield return name;
            }
        }

        await Task.CompletedTask;
    }

    private string PathOf(FileArea area, string key)
    {
        FileKeys.Require(area, key);
        return Path.Combine(_directories.Of(area), key);
    }
}

internal static class FileStoreRegistration
{
    public static IServiceCollection AddFileStore(this IServiceCollection services)
    {
        return services.AddFileStore(FileStoreDirectories.Default);
    }

    public static IServiceCollection AddFileStore(this IServiceCollection services, FileStoreDirectories directories)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(directories);
        services.AddSingleton(directories);
        services.AddSingleton<IFileStore, DiskFileStore>();
        return services;
    }
}
