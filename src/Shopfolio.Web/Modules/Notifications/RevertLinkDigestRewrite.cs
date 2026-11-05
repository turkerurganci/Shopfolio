using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// How a link e-mail's dispatch record names its link record (06 4.58 "EventValues"; 05 4.4.2): the value list holds one entry named
/// <see cref="LinkRecordIdName"/> whose value is the link record's id in the hyphenated form. The record carries the id and never the key: the
/// key is derived from the id when the body is made (05 6.4.4; K-1081). The one writer and the one reader of that entry live here.
/// </summary>
internal static class LinkEventValues
{
    public const string LinkRecordIdName = "LinkRecordId";

    private sealed record Entry(string Name, string Value);

    /// <summary>The value list of a link e-mail: one entry that names the link record.</summary>
    public static string ForLink(Guid linkRecordId) =>
        JsonSerializer.Serialize<Entry[]>([new Entry(LinkRecordIdName, linkRecordId.ToString("D"))]);

    /// <summary>The link record the value list names; null when the list is empty, malformed or names no id (the reader is tolerant, 09 12 item 5).</summary>
    public static Guid? ReadLinkRecordId(string? eventValues)
    {
        if (string.IsNullOrWhiteSpace(eventValues))
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(eventValues);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (JsonElement entry in document.RootElement.EnumerateArray())
            {
                if (entry.ValueKind == JsonValueKind.Object
                    && entry.TryGetProperty(nameof(Entry.Name), out JsonElement name) && name.ValueKind == JsonValueKind.String
                    && string.Equals(name.GetString(), LinkRecordIdName, StringComparison.Ordinal)
                    && entry.TryGetProperty(nameof(Entry.Value), out JsonElement value) && value.ValueKind == JsonValueKind.String
                    && Guid.TryParse(value.GetString(), out Guid id))
                {
                    return id;
                }
            }
        }
        catch (JsonException)
        {
            // A value list that is not JSON names no link; the row is left as it is.
        }

        return null;
    }
}

/// <summary>The outcome of the rewrite: how many link records got a new digest. It is never refused.</summary>
internal sealed record RevertLinkDigestRewriteResult(int Rewritten) : IOperationResult
{
    public bool IsRejected => false;
}

/// <summary>
/// The start-up rewrite of the pending revert links' digests (05 6.x.20, 6.x.15; K-1137; K-1441): the digest of the link record of every
/// "queued" <c>EmailChangeNotice</c> is written again from the present secret, in one database transaction. With an unchanged secret the value
/// is the same; after the secret rotated, or after a recovery or a return to an earlier moment, the link the body will carry matches its record.
/// A sent notice's record is not touched (its link is already out; a rotation's remaining risk is named in 05 6.x.20), and the other link
/// e-mails are not touched either (their links derive from the new secret and no longer match: the user asks again, K-1133). The step needs no
/// trigger and no trace of the secret, and it works from the digest alone: <see cref="LinkKeys.DigestFor"/> never shows the key.
/// </summary>
internal sealed class RevertLinkDigestRewrite
{
    private readonly AppDbContext _db;
    private readonly ITransactionRunner _runner;
    private readonly LinkKeys _keys;

    public RevertLinkDigestRewrite(AppDbContext db, ITransactionRunner runner, LinkKeys keys)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(runner);
        ArgumentNullException.ThrowIfNull(keys);
        _db = db;
        _runner = runner;
        _keys = keys;
    }

    /// <summary>Writes the digests in one transaction; the work is a lambda written at the call (09 10.7 item 5; D-82).</summary>
    public Task<RevertLinkDigestRewriteResult> RewriteAsync(CancellationToken ct) => _runner.RunAsync(
        async cancel =>
        {
            List<string?> values = await _db.DispatchRecords
                .Where(d => d.DispatchType == DispatchType.EmailChangeNotice && d.DispatchState == DispatchState.Queued)
                .Select(d => d.EventValues)
                .ToListAsync(cancel);

            List<Guid> linkIds = [.. values.Select(LinkEventValues.ReadLinkRecordId).OfType<Guid>().Distinct()];
            if (linkIds.Count == 0)
            {
                return new RevertLinkDigestRewriteResult(0);
            }

            // Only a revert link is rewritten, whatever else the value list names.
            List<LinkRecord> links = await _db.LinkRecords
                .Where(l => linkIds.Contains(l.Id) && l.LinkType == LinkType.EmailChangeRevert)
                .ToListAsync(cancel);

            int rewritten = 0;
            foreach (LinkRecord link in links)
            {
                string digest = _keys.DigestFor(link.Id);
                if (!string.Equals(link.TokenHash, digest, StringComparison.Ordinal))
                {
                    link.TokenHash = digest;
                    rewritten++;
                }
            }

            return new RevertLinkDigestRewriteResult(rewritten);
        },
        ct);
}

/// <summary>
/// The job queue's start gate for the rewrite (09 15.1 item 4; D-96 K1): the queue takes no row - and so no e-mail is sent with a link digest
/// written under an older secret - until the step has run in this process. A gate that finds the step not run runs it, so a database that was
/// out of reach at the start does not leave the queue open without the step, nor shut for good.
/// </summary>
internal sealed class RevertLinkDigestRewriteGate : IJobStartGate
{
    private readonly RevertLinkDigestRewriteStartup _startup;

    public RevertLinkDigestRewriteGate(RevertLinkDigestRewriteStartup startup)
    {
        _startup = startup;
    }

    public Task<bool> IsOpenAsync(CancellationToken ct) => _startup.EnsureRunAsync(ct);
}

/// <summary>
/// Runs <see cref="RevertLinkDigestRewrite"/> at every start (05 6.x.20): it is registered after the start-up checks and before every service
/// that starts the job queue or the e-mail sending, because hosted services start in the order they are registered (a test keeps the order). A
/// database that cannot be reached is not an error here: the start-up database check has already written its warning and the liveness
/// endpoint answers 503; <see cref="HasRun"/> stays false, and the job queue does not take a row until it is true: the queue asks for the step
/// through <see cref="RevertLinkDigestRewriteGate"/>, which runs it once when the database comes back (D-96).
/// </summary>
internal sealed class RevertLinkDigestRewriteStartup : IHostedService, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IServiceScopeFactory _scopeFactory;

    public RevertLinkDigestRewriteStartup(NpgsqlDataSource dataSource, IServiceScopeFactory scopeFactory)
    {
        _dataSource = dataSource;
        _scopeFactory = scopeFactory;
    }

    private readonly SemaphoreSlim _once = new(1, 1);
    private volatile bool _hasRun;

    /// <summary>True once the rewrite has run in this process.</summary>
    public bool HasRun => _hasRun;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await EnsureRunAsync(cancellationToken);
    }

    /// <summary>
    /// Runs the rewrite if it has not run yet and the database can be reached; returns whether it has run. The job queue asks this before it
    /// takes any row (<see cref="RevertLinkDigestRewriteGate"/>; 09 15.1 item 4; D-96): a start at which the database was out of reach left
    /// <see cref="HasRun"/> false, and the step runs once, here, when the database comes back. Callers are serialized, so the step runs at most once.
    /// </summary>
    public async Task<bool> EnsureRunAsync(CancellationToken cancellationToken)
    {
        if (_hasRun)
        {
            return true;
        }

        await _once.WaitAsync(cancellationToken);
        try
        {
            if (_hasRun)
            {
                return true;
            }

            if (!await CanConnectAsync(cancellationToken))
            {
                return false;
            }

            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<RevertLinkDigestRewrite>().RewriteAsync(cancellationToken);
            _hasRun = true;
            return true;
        }
        finally
        {
            _once.Release();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Dispose() => _once.Dispose();

    private async Task<bool> CanConnectAsync(CancellationToken ct)
    {
        try
        {
            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(ct);
            return true;
        }
        catch (NpgsqlException)
        {
            // The exception is not written: its text can carry connection details (the start-up database check says the same).
            return false;
        }
    }
}
