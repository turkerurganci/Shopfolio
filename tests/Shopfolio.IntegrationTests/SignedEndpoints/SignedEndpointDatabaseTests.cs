using System.Data.Common;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.SignedEndpoints;

/// <summary>The collection of the signed endpoint tests (T203): one database copied from the template.</summary>
[CollectionDefinition(SignedEndpointDefinition.Name)]
public sealed class SignedEndpointDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SignedEndpoint";
}

/// <summary>The verified notice of the test endpoint: the provider's transaction reference, the outcome and the amount.</summary>
internal sealed record DatabaseTestNotice(string TransactionId, string Outcome, decimal Amount);

/// <summary>
/// A signed endpoint as the notification endpoint's task will write one (05 4.5.2; 08 3.2 principle 3): its signature is an HMAC-SHA256 with a
/// placeholder key - no real secret -, compared in fixed time; its record step writes the inbound record with <c>ON CONFLICT DO NOTHING</c> and,
/// only when a row was written, the job row that processes it, both in the pipeline's open transaction.
/// </summary>
internal sealed class DatabaseTestEndpoint : ISignedEndpoint<DatabaseTestNotice>
{
    public const string TransactionField = "merchant_oid";
    public const string OutcomeField = "status";
    public const string AmountField = "total_amount";
    public const string SignatureField = "hash";
    public const string EventType = "payment";

    private static readonly byte[] PlaceholderKey = Encoding.UTF8.GetBytes("test-placeholder-signing-material");

    private readonly AppDbContext _db;
    private readonly IJobQueue _jobs;
    private readonly TimeProvider _time;

    public DatabaseTestEndpoint(AppDbContext db, IJobQueue jobs, TimeProvider time)
    {
        _db = db;
        _jobs = jobs;
        _time = time;
    }

    public string Acknowledgement => "OK";

    public static string Sign(string transactionId, string outcome, string amount) =>
        Convert.ToBase64String(HMACSHA256.HashData(PlaceholderKey, Encoding.UTF8.GetBytes(transactionId + outcome + amount)));

    public static string Body(string transactionId = "SF20261105001", string outcome = "success", string amount = "149.90", string? signature = null) =>
        $"{TransactionField}={transactionId}&{OutcomeField}={outcome}&{AmountField}={amount}" +
        $"&{SignatureField}={Uri.EscapeDataString(signature ?? Sign(transactionId, outcome, amount))}";

    public DatabaseTestNotice? Verify(IReadOnlyDictionary<string, string> formFields)
    {
        if (!formFields.TryGetValue(TransactionField, out string? transactionId)
            || !formFields.TryGetValue(OutcomeField, out string? outcome)
            || !formFields.TryGetValue(AmountField, out string? amount))
        {
            return null;
        }

        formFields.TryGetValue(SignatureField, out string? received);
        return SignatureComparison.Matches(Sign(transactionId, outcome, amount), received)
            ? new DatabaseTestNotice(transactionId, outcome, decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture))
            : null;
    }

    public async Task<InboundRecordOutcome> RecordAsync(DatabaseTestNotice notice, CancellationToken ct)
    {
        Guid id = Guid.NewGuid();
        int written = await _db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO inbound_notification_records (id, created_at, provider_transaction_id, event_type, outcome, amount)
            VALUES ({id}, {_time.GetUtcNow()}, {notice.TransactionId}, {EventType}, {notice.Outcome}, {notice.Amount})
            ON CONFLICT DO NOTHING
            """,
            ct);
        if (written == 0)
        {
            return new InboundRecordOutcome.AlreadyRecorded();
        }

        await _jobs.EnqueueAsync(JobType.InboundNotificationProcessing, id, notBefore: null, ct);
        return new InboundRecordOutcome.Recorded();
    }
}

/// <summary>Runs the test's step at the commit of every transaction: a look from outside the transaction, or a database error.</summary>
internal sealed class CommitStep : DbTransactionInterceptor
{
    public Func<CancellationToken, Task>? BeforeCommit { get; set; }

    public override async ValueTask<InterceptionResult> TransactionCommittingAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (BeforeCommit is { } step)
        {
            await step(cancellationToken);
        }

        return result;
    }
}

/// <summary>
/// The signed endpoint pipeline on a real PostgreSQL behind a test server (T203 criteria 1, 3, 5, 6, 7, 9; KK1, KK3, KK5, KK6, KK7, KK9; 08 3.2;
/// 07 4.30.1, 2.5.5): the application's own registration (<see cref="SignedEndpointRegistration"/>), transaction runner and job queue, with a test
/// endpoint that records as the notification endpoint will. Every check of a row is made from a separate connection, after the answer.
/// </summary>
[Collection(SignedEndpointDefinition.Name)]
public sealed class SignedEndpointDatabaseTests : IAsyncLifetime
{
    private const string SignedPath = "/test/signed";
    private const string FormPath = "/test/form";

    private readonly CollectionDatabaseFixture _collection;
    private readonly CommitStep _commit = new();
    private readonly CollectingLoggerProvider _logs = new();
    private WebApplication? _app;
    private HttpClient? _client;

    public SignedEndpointDatabaseTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private HttpClient Client => _client!;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE inbound_notification_records, jobs CASCADE");

        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders().AddProvider(_logs);
        builder.Services.AddSingleton<TimeProvider>(new ManualTimeProvider(new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.Zero)));
        builder.Services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn).AddInterceptors(_commit));
        builder.Services.AddScoped<ITransactionRunner, TransactionRunner>();
        builder.Services.AddScoped<JobEnqueueTracker>();
        builder.Services.AddScoped<IJobQueue, JobQueue>();
        builder.Services.AddSingleton<IJobSignal, JobSignal>();
        builder.Services.AddScoped<DatabaseTestEndpoint>();
        builder.Services.AddAntiforgery();
        builder.Services.AddSignedEndpoints();
        builder.Services.AddSingleton<UnexpectedErrorCounter>();
        builder.Services.AddSingleton<UnexpectedErrorLayer>();

        _app = builder.Build();
        _app.UseUnexpectedErrorLayer();
        _app.UseAntiforgery();
        _app.MapSignedEndpoint<DatabaseTestEndpoint, DatabaseTestNotice>(SignedPath);

        // A form endpoint of the same application that keeps the forgery protection: without a token it is refused, so the signed endpoint's
        // answer to a tokenless request shows that it is outside the protection, not that the protection is missing.
        _app.MapPost(FormPath, ([FromForm] string merchant_oid) => merchant_oid);

        await _app.StartAsync(Ct);
        _client = _app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.StopAsync(Ct);
            await _app.DisposeAsync();
        }

        _logs.Dispose();
    }

    [Fact]
    public async Task ValidSignature_IsRecordedWithItsJobRow_AndReceivedOnlyAfterTheCommit()
    {
        long seenFromOutsideAtTheCommit = -1;
        _commit.BeforeCommit = async ct => seenFromOutsideAtTheCommit = await CountAsync("inbound_notification_records", ct);

        using HttpResponseMessage response = await PostAsync(DatabaseTestEndpoint.Body());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("OK", await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal(0, seenFromOutsideAtTheCommit);
        Assert.Equal(1, await CountAsync("inbound_notification_records", Ct));
        Guid recordId = await ScalarAsync<Guid>("SELECT id FROM inbound_notification_records", Ct);
        Assert.Equal(
            1,
            await ScalarAsync<long>(
                $"SELECT count(*) FROM jobs WHERE job_type = '{nameof(JobType.InboundNotificationProcessing)}' AND related_record_id = '{recordId}'",
                Ct));
    }

    [Fact]
    public async Task SameNoticeAgain_IsReceivedWithTheSameAnswer_AndWritesNoSecondRecordOrJobRow()
    {
        using HttpResponseMessage first = await PostAsync(DatabaseTestEndpoint.Body());
        string firstBody = await first.Content.ReadAsStringAsync(Ct);

        using HttpResponseMessage again = await PostAsync(DatabaseTestEndpoint.Body());

        Assert.Equal(first.StatusCode, again.StatusCode);
        Assert.Equal(firstBody, await again.Content.ReadAsStringAsync(Ct));
        Assert.Equal(1, await CountAsync("inbound_notification_records", Ct));
        Assert.Equal(1, await CountAsync("jobs", Ct));
    }

    [Theory]
    [InlineData("wrong")]
    [InlineData("")]
    public async Task BrokenOrEmptySignature_Gets400WithAnEmptyBody_AndNoRecordOrJobRow(string signature)
    {
        using HttpResponseMessage response = await PostAsync(DatabaseTestEndpoint.Body(signature: signature));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertEmptyAndNothingRecordedAsync(response);
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.SignatureFailureSampled, StringComparison.Ordinal));
    }

    [Fact]
    public async Task MissingSignature_Gets400WithAnEmptyBody_AndNoRecordOrJobRow()
    {
        string unsigned = $"{DatabaseTestEndpoint.TransactionField}=SF20261105001&{DatabaseTestEndpoint.OutcomeField}=success&{DatabaseTestEndpoint.AmountField}=149.90";

        using HttpResponseMessage response = await PostAsync(unsigned);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertEmptyAndNothingRecordedAsync(response);
    }

    [Fact]
    public async Task BodyAboveTheLimit_Gets413_AndNoRecordOrJobRow()
    {
        string signed = DatabaseTestEndpoint.Body();
        string over = signed + "&pad=" + new string('a', SignedEndpointLimits.BodyMaxBytes + 1 - signed.Length - "&pad=".Length);
        Assert.Equal(SignedEndpointLimits.BodyMaxBytes + 1, Encoding.UTF8.GetByteCount(over));

        using HttpResponseMessage response = await PostAsync(over);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        await AssertEmptyAndNothingRecordedAsync(response);
        // T213 KK6: the 413 carries no code in its headers either (the body is empty, asserted above).
        string headers = string.Join('\n', response.Headers.Select(h => h.Key + ": " + string.Join(',', h.Value)));
        Assert.All(Enum.GetNames<ErrorCode>(), name => Assert.DoesNotContain(name, headers, StringComparison.Ordinal));
    }

    // A notice that cannot be recorded is never "received" (05 4.5.2; 07 4.30.1): the pipeline does not catch the database error, it leaves the
    // endpoint for the single catching layer (09 8.1 items 3, 6, 7; T212, D-107), whose machine endpoint row answers 500 with an empty body, writes
    // the error line (state MachineEndpoint, code UnexpectedOperationError) and feeds the counter of the status endpoint. The endpoint is a machine
    // endpoint by the metadata MapSignedEndpoint writes, not by its path; nothing was recorded and no job row was opened.
    [Fact]
    public async Task DatabaseErrorOnTheCommit_LeavesTheEndpointForTheCatchingLayer_AndNothingIsRecorded()
    {
        _commit.BeforeCommit = _ => throw new NpgsqlException("test commit failure");
        int before = _app!.Services.GetRequiredService<UnexpectedErrorCounter>().CountWithin(TimeSpan.FromMinutes(15));

        using HttpResponseMessage response = await PostAsync(DatabaseTestEndpoint.Body());

        _commit.BeforeCommit = null;
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(Ct));
        await AssertNothingRecordedAsync();
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.UnexpectedError, StringComparison.Ordinal)
            && line.Contains(nameof(ErrorCode.UnexpectedOperationError), StringComparison.Ordinal)
            && line.Contains(nameof(ErrorState.MachineEndpoint), StringComparison.Ordinal));
        Assert.Equal(before + 1, _app.Services.GetRequiredService<UnexpectedErrorCounter>().CountWithin(TimeSpan.FromMinutes(15)));
    }

    // D-84: the statement time-out is the server's own error (SQLSTATE 57014), made here by a real statement that runs past a short limit on a separate
    // connection while the commit is waiting - not a thrown stand-in. It leaves the endpoint as it is and the machine endpoint row answers 500 without a body.
    [Fact]
    public async Task StatementTimeOutOnTheCommit_Is500WithoutABody_TheMachineEndpointLine_AndTheCounter_D84()
    {
        PostgresException? raised = null;
        _commit.BeforeCommit = async ct =>
        {
            await using NpgsqlConnection other = new(_collection.Dsn);
            await other.OpenAsync(ct);
            await using NpgsqlCommand command = new("SET statement_timeout = '100ms'; SELECT pg_sleep(5)", other);
            raised = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync(ct));
            throw raised;
        };
        int before = _app!.Services.GetRequiredService<UnexpectedErrorCounter>().CountWithin(TimeSpan.FromMinutes(15));

        using HttpResponseMessage response = await PostAsync(DatabaseTestEndpoint.Body());

        _commit.BeforeCommit = null;
        Assert.Equal(PostgresErrorCodes.QueryCanceled, raised?.SqlState);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(Ct));
        await AssertNothingRecordedAsync();
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.UnexpectedError, StringComparison.Ordinal)
            && line.Contains(nameof(ErrorCode.UnexpectedOperationError), StringComparison.Ordinal)
            && line.Contains(nameof(ErrorState.MachineEndpoint), StringComparison.Ordinal));
        Assert.Equal(before + 1, _app.Services.GetRequiredService<UnexpectedErrorCounter>().CountWithin(TimeSpan.FromMinutes(15)));
    }

    [Fact]
    public async Task RecordThatBreaksARule_LeavesTheEndpointForTheCatchingLayer_AndNothingIsRecorded()
    {
        // The record's amount must not be negative (06 4.35, amount_not_negative): the insert fails in the database.
        using HttpResponseMessage response = await PostAsync(DatabaseTestEndpoint.Body(amount: "-1.00"));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync(Ct));
        await AssertNothingRecordedAsync();
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.UnexpectedError, StringComparison.Ordinal)
            && line.Contains(nameof(ErrorState.MachineEndpoint), StringComparison.Ordinal));
    }

    [Fact]
    public async Task TokenlessPost_IsHandledBySignedEndpoint_WhileTheProtectedFormOfTheSameAppIsRefused()
    {
        using HttpResponseMessage protectedForm = await PostAsync(DatabaseTestEndpoint.Body(), FormPath);
        using HttpResponseMessage signed = await PostAsync(DatabaseTestEndpoint.Body());

        Assert.Equal(HttpStatusCode.BadRequest, protectedForm.StatusCode);
        Assert.Equal(HttpStatusCode.OK, signed.StatusCode);
        Assert.Equal(1, await CountAsync("inbound_notification_records", Ct));
    }

    [Fact]
    public async Task OtherMethodThanPost_IsNotTheSignedEndpoint()
    {
        using HttpResponseMessage response = await Client.GetAsync(new Uri(SignedPath, UriKind.Relative), Ct);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await CountAsync("inbound_notification_records", Ct));
    }

    private async Task AssertEmptyAndNothingRecordedAsync(HttpResponseMessage response)
    {
        Assert.Empty(await response.Content.ReadAsByteArrayAsync(Ct));
        await AssertNothingRecordedAsync();
    }

    private async Task AssertNothingRecordedAsync()
    {
        Assert.Equal(0, await CountAsync("inbound_notification_records", Ct));
        Assert.Equal(0, await CountAsync("jobs", Ct));
    }

    private Task<HttpResponseMessage> PostAsync(string body, string path = SignedPath)
    {
        StringContent content = new(body, Encoding.UTF8, "application/x-www-form-urlencoded");
        return Client.PostAsync(new Uri(path, UriKind.Relative), content, Ct);
    }

    private Task<long> CountAsync(string table, CancellationToken ct) => ScalarAsync<long>($"SELECT count(*) FROM {table}", ct);

    private async Task<T> ScalarAsync<T>(string sql, CancellationToken ct)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(ct))!;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
