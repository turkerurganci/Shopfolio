using System.Runtime.CompilerServices;

using Shopfolio.Infrastructure.Files;

namespace Shopfolio.UnitTests.Fakes;

/// <summary>
/// The fake of the file store (09 19.1 item 3; 05 7.7.1): the same contract as the disk implementation - the key's shape is checked, a write
/// replaces, an append extends, a missing file has size zero - kept in memory with the moment each file was written, so a test can age a file with
/// the fake clock. It touches no directory.
/// </summary>
internal sealed class InMemoryFileStore : IFileStore
{
    private readonly Dictionary<(FileArea Area, string Key), (byte[] Content, DateTimeOffset WrittenAt)> _files = [];
    private readonly TimeProvider _time;

    public InMemoryFileStore(TimeProvider time)
    {
        _time = time;
    }

    /// <summary>The calls in order, as <c>Operation:Area:Key</c>; a test reads the order of a record and a file deletion from the log it shares.</summary>
    public List<string> Calls { get; } = [];

    public IReadOnlyList<string> KeysOf(FileArea area) => [.. _files.Keys.Where(k => k.Area == area).Select(k => k.Key).Order(StringComparer.Ordinal)];

    public byte[] ContentOf(FileArea area, string key) => _files[(area, key)].Content;

    /// <summary>Puts a file in with a given age, as the disk would show after time has passed.</summary>
    public void Seed(FileArea area, string key, byte[] content, DateTimeOffset writtenAt)
    {
        FileKeys.Require(area, key);
        _files[(area, key)] = (content, writtenAt);
    }

    public async Task WriteAsync(FileArea area, string key, Stream content, CancellationToken ct)
    {
        FileKeys.Require(area, key);
        Calls.Add($"Write:{area}:{key}");
        using MemoryStream copy = new();
        await content.CopyToAsync(copy, ct);
        _files[(area, key)] = (copy.ToArray(), _time.GetUtcNow());
    }

    public async Task AppendAsync(FileArea area, string key, Stream content, CancellationToken ct)
    {
        FileKeys.Require(area, key);
        if (area != FileArea.DigitalFile)
        {
            throw new ArgumentException("Only a digital file is uploaded in chunks.", nameof(area));
        }

        Calls.Add($"Append:{area}:{key}");
        using MemoryStream copy = new();
        await content.CopyToAsync(copy, ct);
        byte[] before = _files.TryGetValue((area, key), out (byte[] Content, DateTimeOffset WrittenAt) existing) ? existing.Content : [];
        _files[(area, key)] = ([.. before, .. copy.ToArray()], _time.GetUtcNow());
    }

    public Task<Stream> OpenReadAsync(FileArea area, string key, CancellationToken ct)
    {
        FileKeys.Require(area, key);
        return Task.FromResult<Stream>(new MemoryStream(_files[(area, key)].Content, writable: false));
    }

    public Task<bool> ExistsAsync(FileArea area, string key, CancellationToken ct)
    {
        FileKeys.Require(area, key);
        return Task.FromResult(_files.ContainsKey((area, key)));
    }

    public Task<long> GetSizeAsync(FileArea area, string key, CancellationToken ct)
    {
        FileKeys.Require(area, key);
        return Task.FromResult(_files.TryGetValue((area, key), out (byte[] Content, DateTimeOffset WrittenAt) file) ? (long)file.Content.Length : 0L);
    }

    public Task DeleteAsync(FileArea area, string key, CancellationToken ct)
    {
        FileKeys.Require(area, key);
        Calls.Add($"Delete:{area}:{key}");
        _files.Remove((area, key));
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> ListAsync(FileArea area, TimeSpan minAge, [EnumeratorCancellation] CancellationToken ct)
    {
        DateTimeOffset cutoff = _time.GetUtcNow() - minAge;
        foreach (string key in _files.Where(f => f.Key.Area == area && f.Value.WrittenAt <= cutoff).Select(f => f.Key.Key).Order(StringComparer.Ordinal).ToList())
        {
            yield return key;
        }

        await Task.CompletedTask;
    }
}
