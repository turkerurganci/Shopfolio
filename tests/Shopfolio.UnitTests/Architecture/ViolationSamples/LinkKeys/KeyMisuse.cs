// Deliberate violation sample of the key and link scan (09 15.1 items 1, 3 and 4). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test.
internal static class KeyMisuse
{
    public static byte[] Bytes() => RandomNumberGenerator.GetBytes(32);
    public static string Digest(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
    public static string Mac(byte[] k, byte[] d) => Convert.ToHexString(HMACSHA256.HashData(k, d));
    public static Guid Weak() => Guid.NewGuid();
    public static Guid WeakToo() => Guid.CreateVersion7();
    public static int WeakRandom() => new Random().Next();
    public static int WeakShared() => Random.Shared.Next();
    // A comment that says RandomNumberGenerator or keys.Derive(id) or TryConsumeAsync( is not a finding.
    public static string Key(LinkKeys keys, Guid id) => keys.Derive(id);
    public static string Wrapped(LinkKeys keys, Guid id) => keys
        .Derive(id);
    public async Task OnGetAsync(CancellationToken ct)
    {
        await _links.TryConsumeAsync(_id, ct);
    }
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await runner.RunAsync(async token => await _links.TryConsumeAsync(_id, token), ct);
    }
    public async Task OnGetInner(CancellationToken ct)
    {
        async Task Helper() { await _links.TryConsumeAsync(_id, ct); }
        await Helper();
    }
    public Task<bool> Elsewhere(CancellationToken ct) => _links.TryConsumeAsync(_id, ct);
    public Task<bool> Split() =>
        _links.TryConsumeAsync(
            _id, default);
}
