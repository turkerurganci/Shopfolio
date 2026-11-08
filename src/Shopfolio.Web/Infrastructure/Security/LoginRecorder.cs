using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>The result of the recorder's own short transaction: never refused; a failure is an exception.</summary>
internal sealed record LoginWriteResult(Guid LoginRecordId) : IOperationResult
{
    public bool IsRejected => false;
}

/// <summary>
/// The writer of the sign-in record (02 3.13.19, 3.13.20; 06 4.44; T341): every attempt, successful or not, customer or administrator, is one row with its moment, IP, e-mail
/// and outcome, and this is the only code that makes one (a source scan keeps <see cref="LoginRecord.Create"/> in this file and in the record's own). Like the attempt counter,
/// a row is written in a short transaction of its own, on a connection of its own, and committed at once: a caller that holds an open transaction, or one that rolls back
/// because the sign-in was refused, leaves the row where it is. An unknown client address is written as <see cref="IPAddress.None"/>.
/// </summary>
internal sealed class LoginRecorder
{
    private readonly IServiceScopeFactory _scopes;
    private readonly TimeProvider _time;

    public LoginRecorder(IServiceScopeFactory scopes, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        ArgumentNullException.ThrowIfNull(time);
        _scopes = scopes;
        _time = time;
    }

    /// <summary>Writes one row for the attempt in a transaction of its own and commits it; returns the row's identifier.</summary>
    public async Task<Guid> RecordAsync(IPAddress? ipAddress, AccountType accountType, string email, LoginOutcome outcome, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        LoginRecord row = LoginRecord.Create(ipAddress ?? IPAddress.None, accountType, email, outcome, _time.GetUtcNow());
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        await runner.RunAsync(
            _ =>
            {
                db.LoginRecords.Add(row);
                return Task.FromResult(new LoginWriteResult(row.Id));
            },
            ct);
        return row.Id;
    }
}
