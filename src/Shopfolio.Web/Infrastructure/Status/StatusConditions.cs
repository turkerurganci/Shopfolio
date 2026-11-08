using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Status;

/// <summary>
/// One warning condition of the status endpoint (05 9.4.2). It reads its source and answers whether the condition is open; a source it cannot read
/// throws, and the evaluator counts that as open (05 9.4.10, K-975). The name is for the server's status command (05 8.5.6) and never reaches a response.
/// </summary>
internal interface IStatusCondition
{
    string Name { get; }

    /// <summary>
    /// True when the condition reads the web process's own memory (the error counter, the payment start series, the certificate seen by the heartbeat): another process - the server's
    /// status command - has no such memory and does not evaluate it (05 8.5.6, 9.4.10).
    /// </summary>
    bool ReadsProcessMemory => false;

    Task<bool> IsOpenAsync(CancellationToken ct);
}

internal sealed class DiskUsageCondition : IStatusCondition
{
    private readonly IDiskUsageReader _disk;
    private readonly IOptions<StatusThresholdOptions> _options;

    public DiskUsageCondition(IDiskUsageReader disk, IOptions<StatusThresholdOptions> options)
    {
        _disk = disk;
        _options = options;
    }

    public string Name => "DiskUsage";

    public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(StatusRules.DiskFull(_disk.UsedFraction(), _options.Value));
}

internal sealed class LogShippingCondition : IStatusCondition
{
    private readonly ILogShippingReader _reader;
    private readonly TimeProvider _time;
    private readonly IOptions<StatusThresholdOptions> _options;

    public LogShippingCondition(ILogShippingReader reader, TimeProvider time, IOptions<StatusThresholdOptions> options)
    {
        _reader = reader;
        _time = time;
        _options = options;
    }

    public string Name => "LogShipping";

    public async Task<bool> IsOpenAsync(CancellationToken ct) =>
        StatusRules.LogShippingStalled(await _reader.ReadAsync(ct), _time.GetUtcNow(), _options.Value);
}

/// <summary>The nightly full backup into the repository outside the server (05 9.4.5); the local copy is no success and is not recorded here (06 4.67).</summary>
internal sealed class FullBackupCondition : IStatusCondition
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly IOptions<StatusThresholdOptions> _options;

    public FullBackupCondition(AppDbContext db, TimeProvider time, IOptions<StatusThresholdOptions> options)
    {
        _db = db;
        _time = time;
        _options = options;
    }

    public string Name => "FullBackup";

    public async Task<bool> IsOpenAsync(CancellationToken ct)
    {
        DateTimeOffset? last = await _db.BackupStatusRecords.AsNoTracking().Select(b => b.LastSucceededAt).SingleAsync(ct);
        return StatusRules.BackupTooOld(last, _time.GetUtcNow(), _options.Value);
    }
}

/// <summary>The nightly file backup (05 9.4.5); a separate condition from the full backup, so either one opens the status alone.</summary>
internal sealed class FileBackupCondition : IStatusCondition
{
    private readonly IFileBackupStatusReader _reader;
    private readonly TimeProvider _time;
    private readonly IOptions<StatusThresholdOptions> _options;

    public FileBackupCondition(IFileBackupStatusReader reader, TimeProvider time, IOptions<StatusThresholdOptions> options)
    {
        _reader = reader;
        _time = time;
        _options = options;
    }

    public string Name => "FileBackup";

    public async Task<bool> IsOpenAsync(CancellationToken ct) =>
        StatusRules.BackupTooOld(await _reader.ReadLastSuccessAsync(ct), _time.GetUtcNow(), _options.Value);
}

/// <summary>The heartbeat (05 9.4.6): the scheduler stopped or its self-check failed.</summary>
internal sealed class HeartbeatCondition : IStatusCondition
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly IOptions<StatusThresholdOptions> _options;

    public HeartbeatCondition(AppDbContext db, TimeProvider time, IOptions<StatusThresholdOptions> options)
    {
        _db = db;
        _time = time;
        _options = options;
    }

    public string Name => "Heartbeat";

    public async Task<bool> IsOpenAsync(CancellationToken ct)
    {
        DateTimeOffset? last = await _db.HeartbeatRecords.AsNoTracking().Select(h => h.LastHeartbeatAt).SingleAsync(ct);
        return StatusRules.HeartbeatTooOld(last, _time.GetUtcNow(), _options.Value);
    }
}

/// <summary>The nightly destruction and clean-up (05 9.4.17): the last success of the nightly job (05 5.1.1).</summary>
internal sealed class NightlyJobCondition : IStatusCondition
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly IOptions<StatusThresholdOptions> _options;

    public NightlyJobCondition(AppDbContext db, TimeProvider time, IOptions<StatusThresholdOptions> options)
    {
        _db = db;
        _time = time;
        _options = options;
    }

    public string Name => "NightlyJob";

    public async Task<bool> IsOpenAsync(CancellationToken ct)
    {
        DateTimeOffset? last = await _db.HeartbeatRecords.AsNoTracking().Select(h => h.NightlyJobLastSucceededAt).SingleAsync(ct);
        return StatusRules.NightlyJobTooOld(last, _time.GetUtcNow(), _options.Value);
    }
}

/// <summary>The unexpected errors of the last window (05 9.4.8): the memory counter of the catching layer; only 500s are in it (07 2.7.5).</summary>
internal sealed class UnexpectedErrorCondition : IStatusCondition
{
    private readonly UnexpectedErrorCounter _counter;
    private readonly IOptions<StatusThresholdOptions> _options;

    public UnexpectedErrorCondition(UnexpectedErrorCounter counter, IOptions<StatusThresholdOptions> options)
    {
        _counter = counter;
        _options = options;
    }

    public string Name => "UnexpectedErrors";

    public bool ReadsProcessMemory => true;

    public Task<bool> IsOpenAsync(CancellationToken ct)
    {
        StatusThresholdOptions options = _options.Value;
        int count = _counter.CountWithin(TimeSpan.FromMinutes(options.UnexpectedErrorWindowMinutes));
        return Task.FromResult(StatusRules.UnexpectedErrorsHigh(count, options));
    }
}

/// <summary>The payment start's failure series (05 9.4.18), from its memory record.</summary>
internal sealed class PaymentStartFailureCondition : IStatusCondition
{
    private readonly PaymentStartFailureSeries _series;
    private readonly TimeProvider _time;
    private readonly IOptions<StatusThresholdOptions> _options;

    public PaymentStartFailureCondition(PaymentStartFailureSeries series, TimeProvider time, IOptions<StatusThresholdOptions> options)
    {
        _series = series;
        _time = time;
        _options = options;
    }

    public string Name => "PaymentStartFailures";

    public bool ReadsProcessMemory => true;

    public Task<bool> IsOpenAsync(CancellationToken ct)
    {
        (int count, DateTimeOffset? firstAt) = _series.Snapshot();
        return Task.FromResult(StatusRules.PaymentStartSeriesOpen(count, firstAt, _time.GetUtcNow(), _options.Value));
    }
}

/// <summary>The site's certificate (05 9.4.9), as the heartbeat's request saw its end.</summary>
internal sealed class CertificateCondition : IStatusCondition
{
    private readonly CertificateExpiryMemory _memory;
    private readonly TimeProvider _time;
    private readonly IOptions<StatusThresholdOptions> _options;

    public CertificateCondition(CertificateExpiryMemory memory, TimeProvider time, IOptions<StatusThresholdOptions> options)
    {
        _memory = memory;
        _time = time;
        _options = options;
    }

    public string Name => "Certificate";

    public bool ReadsProcessMemory => true;

    public Task<bool> IsOpenAsync(CancellationToken ct) =>
        Task.FromResult(StatusRules.CertificateExpiring(_memory.NotAfter, _time.GetUtcNow(), _options.Value));
}
