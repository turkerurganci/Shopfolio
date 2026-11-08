using System.Data.Common;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

using Shopfolio.Infrastructure.Status;
using Shopfolio.Modules.Notifications;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The evaluator's treatment of an unreadable source (T314 criterion 10, KK10; 05 9.4.10, K-975), the channel warning rule (KK6), the status file's
/// text and the certificate memory (KK8). No file, no network and no database is touched; a condition is a scripted fake.
/// </summary>
public sealed class StatusEvaluatorTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // --- The evaluator ---

    [Fact]
    public async Task NoCondition_IsNoWarning()
    {
        StatusEvaluator evaluator = new([]);

        Assert.Empty(await evaluator.OpenConditionsAsync(Ct));
    }

    [Fact]
    public async Task OnlyTheOpenConditionsAreNamed_InRegistrationOrder()
    {
        StatusEvaluator evaluator = new([new Scripted("A", false), new Scripted("B", true), new Scripted("C", true)]);

        Assert.Equal(["B", "C"], await evaluator.OpenConditionsAsync(Ct));
    }

    public static TheoryData<string> ReadFailures => ["db", "io", "access", "json", "format", "invalid", "timeout", "cancelled-by-source"];

    [Theory]
    [MemberData(nameof(ReadFailures))]
    public async Task ASourceThatCannotBeRead_IsAnOpenCondition_NotAnError(string failure)
    {
        StatusEvaluator evaluator = new([new Scripted("Source", () => throw Failure(failure)), new Scripted("Other", false)]);

        Assert.Equal(["Source"], await evaluator.OpenConditionsAsync(Ct));
    }

    [Fact]
    public async Task ACancelledRequest_PassesUpAsACancellation()
    {
        using CancellationTokenSource cancelled = new();
        await cancelled.CancelAsync();
        StatusEvaluator evaluator = new([new Scripted("Source", () => throw new OperationCanceledException(cancelled.Token))]);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => evaluator.OpenConditionsAsync(cancelled.Token));
    }

    [Fact]
    public async Task AnUnknownFailure_IsNotHiddenAsAWarning_ItPassesUp()
    {
        StatusEvaluator evaluator = new([new Scripted("Source", () => throw new NotImplementedException())]);

        await Assert.ThrowsAsync<NotImplementedException>(() => evaluator.OpenConditionsAsync(Ct));
    }

    // --- The status command's standings (T389 criterion 6; 05 8.5.6) ---

    [Fact]
    public async Task Standings_ListTheOpenAndTheClosedConditions_InRegistrationOrder()
    {
        StatusEvaluator evaluator = new([new Scripted("A", false), new Scripted("B", true)]);

        IReadOnlyList<StatusConditionStanding> standings = await evaluator.StandingsAsync(Ct);

        Assert.Equal(["A", "B"], standings.Select(s => s.Name));
        Assert.Equal([false, true], standings.Select(s => s.IsOpen));
        Assert.All(standings, s => Assert.False(s.NotMeasurable || s.SourceUnreadable));
    }

    [Fact]
    public async Task Standings_AConditionThatReadsTheWebProcessMemory_IsNotEvaluated_AndIsNeitherOpenNorClosed()
    {
        bool evaluated = false;
        StatusEvaluator evaluator = new([new Scripted("Memory", () => evaluated = true) { ReadsProcessMemory = true }]);

        StatusConditionStanding standing = Assert.Single(await evaluator.StandingsAsync(Ct));

        Assert.False(evaluated);
        Assert.True(standing.NotMeasurable);
        Assert.False(standing.IsOpen);
    }

    [Fact]
    public async Task Standings_AnUnreadableSource_IsOpenAndMarkedUnreadable_AndTheEndpointListStaysTheSame()
    {
        StatusEvaluator evaluator = new([new Scripted("Source", () => throw new IOException()), new Scripted("Other", false)]);

        IReadOnlyList<StatusConditionStanding> standings = await evaluator.StandingsAsync(Ct);

        Assert.True(standings[0].IsOpen);
        Assert.True(standings[0].SourceUnreadable);
        Assert.False(standings[1].IsOpen);
        Assert.Equal(["Source"], await evaluator.OpenConditionsAsync(Ct));
    }

    [Fact]
    public void TheThreeMemoryConditions_AreMarkedAsReadingTheProcessMemory_AndTheOthersAreNot()
    {
        Assert.True(typeof(UnexpectedErrorCondition).GetProperty(nameof(IStatusCondition.ReadsProcessMemory))!.DeclaringType == typeof(UnexpectedErrorCondition));
        Assert.True(typeof(PaymentStartFailureCondition).GetProperty(nameof(IStatusCondition.ReadsProcessMemory))!.DeclaringType == typeof(PaymentStartFailureCondition));
        Assert.True(typeof(CertificateCondition).GetProperty(nameof(IStatusCondition.ReadsProcessMemory))!.DeclaringType == typeof(CertificateCondition));
        Assert.Null(typeof(DiskUsageCondition).GetProperty(nameof(IStatusCondition.ReadsProcessMemory)));
    }

    private static Exception Failure(string kind) => kind switch
    {
        "db" => new TestDbException(),
        "io" => new IOException(),
        "access" => new UnauthorizedAccessException(),
        "json" => new JsonException(),
        "format" => new FormatException(),
        "invalid" => new InvalidOperationException(),
        "timeout" => new TimeoutException(),
        _ => new OperationCanceledException(),
    };

    // --- KK6: the e-mail channel warning ---

    [Fact]
    public void ChannelWarning_FewerErrorsThanTheCount_IsClosed_EvenWhenOld()
    {
        DateTimeOffset first = _now.AddMinutes(-(ChannelWarningCondition.FirstErrorAgeMinutes + 5));

        Assert.False(ChannelWarningCondition.IsOpen(ChannelWarningCondition.ErrorsInARow - 1, first, _now));
    }

    [Fact]
    public void ChannelWarning_EnoughErrorsButTheFirstIsYounger_IsClosed_ThenOpen()
    {
        DateTimeOffset age = _now.AddMinutes(-ChannelWarningCondition.FirstErrorAgeMinutes);

        Assert.False(ChannelWarningCondition.IsOpen(ChannelWarningCondition.ErrorsInARow, age.AddSeconds(1), _now));
        Assert.True(ChannelWarningCondition.IsOpen(ChannelWarningCondition.ErrorsInARow, age, _now));
    }

    [Fact]
    public void ChannelWarning_AfterASuccess_TheCountIsZeroAndItIsClosed()
    {
        Assert.False(ChannelWarningCondition.IsOpen(0, null, _now));
    }

    // --- The file backup's status file (deploy/images/shared/write-status.sh) ---

    [Fact]
    public void StatusFile_ReadsTheLastSuccess_AsAnInstant()
    {
        DateTimeOffset? read = JsonFileBackupStatusReader.ParseLastSuccess("""{"result":"failure","lastSuccessAt":"2026-11-06T03:10:00Z"}""");

        Assert.Equal(new DateTimeOffset(2026, 11, 6, 3, 10, 0, TimeSpan.Zero), read);
    }

    [Fact]
    public void StatusFile_WithTheEmptyText_IsNoSuccessYet()
    {
        Assert.Null(JsonFileBackupStatusReader.ParseLastSuccess("""{"lastSuccessAt":""}"""));
    }

    [Theory]
    [InlineData("""{"result":"success"}""")]
    [InlineData("""{"lastSuccessAt":12}""")]
    [InlineData("""{"lastSuccessAt":"yesterday"}""")]
    [InlineData("[]")]
    public void StatusFile_OfAnotherShape_IsUnreadable(string text)
    {
        Assert.ThrowsAny<Exception>(() => JsonFileBackupStatusReader.ParseLastSuccess(text));
    }

    [Fact]
    public void StatusFile_NotJson_IsUnreadable()
    {
        Assert.ThrowsAny<JsonException>(() => JsonFileBackupStatusReader.ParseLastSuccess("not json"));
    }

    // --- KK8: the certificate memory ---

    [Fact]
    public void CertificateMemory_IsEmptyBeforeAnyHandshake()
    {
        Assert.Null(new CertificateExpiryMemory().NotAfter);
    }

    [Fact]
    public void CertificateMemory_KeepsTheEndOfTheCertificate_AndLeavesTheVerdictToTheValidation()
    {
        using RSA key = RSA.Create(2048);
        CertificateRequest request = new("CN=status-test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        DateTimeOffset notAfter = new(2027, 1, 15, 0, 0, 0, TimeSpan.Zero);
        using X509Certificate2 certificate = request.CreateSelfSigned(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero), notAfter);
        CertificateExpiryMemory memory = new();

        bool acceptedClean = memory.Observe(certificate, SslPolicyErrors.None);
        bool acceptedBroken = memory.Observe(certificate, SslPolicyErrors.RemoteCertificateChainErrors);

        Assert.True(acceptedClean);
        Assert.False(acceptedBroken);
        Assert.Equal(notAfter, memory.NotAfter);
    }

    [Theory]
    [InlineData(2027)]
    [InlineData(2051)]
    public void NotAfterOf_ReadsBothTimeEncodings_UtcTimeBefore2050_GeneralizedTimeFrom2050(int year)
    {
        using RSA key = RSA.Create(2048);
        CertificateRequest request = new("CN=status-test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        DateTimeOffset notAfter = new(year, 6, 30, 12, 0, 0, TimeSpan.Zero);
        using X509Certificate2 certificate = request.CreateSelfSigned(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero), notAfter);

        Assert.Equal(notAfter, CertificateExpiryMemory.NotAfterOf(certificate.RawData));
    }

    [Fact]
    public void NotAfterOf_OfBytesThatAreNoCertificate_Throws()
    {
        Assert.ThrowsAny<System.Formats.Asn1.AsnContentException>(() => CertificateExpiryMemory.NotAfterOf([1, 2, 3]));
    }

    [Fact]
    public void CertificateMemory_AMissingCertificate_RemembersNothing()
    {
        CertificateExpiryMemory memory = new();

        bool accepted = memory.Observe(null, SslPolicyErrors.RemoteCertificateNotAvailable);

        Assert.False(accepted);
        Assert.Null(memory.NotAfter);
    }

    private sealed class Scripted : IStatusCondition
    {
        private readonly Func<bool> _read;

        public Scripted(string name, bool open)
            : this(name, () => open)
        {
        }

        public Scripted(string name, Func<bool> read)
        {
            Name = name;
            _read = read;
        }

        public string Name { get; }

        public bool ReadsProcessMemory { get; init; }

        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(_read());
    }

    private sealed class TestDbException : DbException
    {
    }
}
