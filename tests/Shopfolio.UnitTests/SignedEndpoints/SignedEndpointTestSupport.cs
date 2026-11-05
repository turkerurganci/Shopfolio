using System.Data.Common;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Http;

using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.SignedEndpoints;

/// <summary>The notice a test endpoint verifies: the order reference and the outcome, nothing personal.</summary>
internal sealed record TestNotice(string OrderReference, string Outcome);

/// <summary>
/// A signed endpoint for the pipeline's tests. Its signature is an HMAC-SHA256 of two fields with a placeholder key - no real secret -, compared
/// with the request's <c>hash</c> field through the layer's fixed-time comparison. It counts every call so a test can tell what ran and in which order.
/// </summary>
internal sealed class TestSignedEndpoint : ISignedEndpoint<TestNotice>
{
    public const string ReferenceField = "reference";
    public const string OutcomeField = "outcome";
    public const string SignatureField = "hash";

    private static readonly byte[] PlaceholderKey = Encoding.UTF8.GetBytes("test-placeholder-signing-material");

    public List<string> Calls { get; } = [];

    public InboundRecordOutcome Outcome { get; set; } = new InboundRecordOutcome.Recorded();

    /// <summary>When set, the record step throws it, as a database error on the write would.</summary>
    public Exception? RecordFailure { get; set; }

    public string Acknowledgement => "OK";

    public static string Sign(string reference, string outcome) =>
        Convert.ToBase64String(HMACSHA256.HashData(PlaceholderKey, Encoding.UTF8.GetBytes(reference + outcome)));

    /// <summary>A URL-encoded body with the two fields and their signature (or the given one), and extra fields when asked.</summary>
    public static string Body(string reference = "order-1", string outcome = "success", string? signature = null, string extra = "")
    {
        string hash = signature ?? Sign(reference, outcome);
        return $"{ReferenceField}={Uri.EscapeDataString(reference)}&{OutcomeField}={outcome}&{SignatureField}={Uri.EscapeDataString(hash)}{extra}";
    }

    public TestNotice? Verify(IReadOnlyDictionary<string, string> formFields)
    {
        Calls.Add(nameof(Verify));
        if (!formFields.TryGetValue(ReferenceField, out string? reference) || !formFields.TryGetValue(OutcomeField, out string? outcome))
        {
            return null;
        }

        formFields.TryGetValue(SignatureField, out string? received);
        return SignatureComparison.Matches(Sign(reference, outcome), received) ? new TestNotice(reference, outcome) : null;
    }

    public Task<InboundRecordOutcome> RecordAsync(TestNotice notice, CancellationToken ct)
    {
        Calls.Add(nameof(RecordAsync));
        return RecordFailure is { } failure ? Task.FromException<InboundRecordOutcome>(failure) : Task.FromResult(Outcome);
    }
}

/// <summary>A database error as the data provider throws it, without a database.</summary>
internal sealed class TestDatabaseException : DbException
{
    public TestDatabaseException()
        : base("test database failure")
    {
    }
}

/// <summary>
/// A transaction runner that counts the transactions, runs the work and then commits - or fails the commit with a database error when told,
/// so the pipeline's answer is shown to wait for the commit.
/// </summary>
internal sealed class TestTransactionRunner : ITransactionRunner
{
    private readonly List<string> _calls;

    public TestTransactionRunner(List<string> calls)
    {
        _calls = calls;
    }

    public bool FailCommit { get; set; }

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)
        where T : IOperationResult
    {
        _calls.Add("Begin");
        T result = await work(ct);
        if (FailCommit)
        {
            _calls.Add("CommitFailed");
            throw new TestDatabaseException();
        }

        _calls.Add("Commit");
        return result;
    }
}

/// <summary>A stream whose every read fails: the body of a request that must not be read.</summary>
internal sealed class UnreadableStream : MemoryStream
{
    public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException("the body was read");

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("the body was read");

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("the body was read");
}

/// <summary>The pipeline on a fake clock, with its log lines kept in memory (09 19.2 item 2: no file is written).</summary>
internal sealed class SignedEndpointHarness : IDisposable
{
    public static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private readonly Logger _root;
    private readonly SerilogLoggerFactory _factory;

    public SignedEndpointHarness()
    {
        _root = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(Sink).CreateLogger();
        _factory = new SerilogLoggerFactory(_root);
        Failures = new SignatureFailureLog(Time, new Microsoft.Extensions.Logging.Logger<SignatureFailureLog>(_factory));
        Handler = new SignedEndpointHandler(Failures, new Microsoft.Extensions.Logging.Logger<SignedEndpointHandler>(_factory));
        Transactions = new TestTransactionRunner(Endpoint.Calls);
    }

    public ManualTimeProvider Time { get; } = new(Now);

    public CollectingSink Sink { get; } = new();

    public SignatureFailureLog Failures { get; }

    public SignedEndpointHandler Handler { get; }

    public TestSignedEndpoint Endpoint { get; } = new();

    public TestTransactionRunner Transactions { get; }

    public IReadOnlyList<string> Lines => [.. Sink.Events.Select(LogCapture.Line)];

    public static DefaultHttpContext Request(string body, string contentType = "application/x-www-form-urlencoded", bool declareLength = true)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        DefaultHttpContext context = new();
        context.Request.Method = HttpMethods.Post;
        context.Request.ContentType = contentType;
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = declareLength ? bytes.Length : null;
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.10");
        context.Response.Body = new MemoryStream();
        return context;
    }

    public async Task<(int Status, string Body, string? ContentType)> SendAsync(DefaultHttpContext context)
    {
        await Handler.HandleAsync(context, Endpoint, Transactions);
        context.Response.Body.Position = 0;
        using StreamReader reader = new(context.Response.Body, Encoding.UTF8);
        return (context.Response.StatusCode, await reader.ReadToEndAsync(TestContext.Current.CancellationToken), context.Response.ContentType);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _root.Dispose();
    }
}
