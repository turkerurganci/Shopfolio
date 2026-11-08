using System.Diagnostics;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Npgsql;

using Shopfolio.Commands;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Commands;

/// <summary>The collection of the command tests (T303): one database copied from the template, made by the real migrations.</summary>
[CollectionDefinition(CreateAdminCommandDefinition.Name)]
public sealed class CreateAdminCommandDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CreateAdminCommand";
}

/// <summary>
/// The <c>create-admin</c> command on a real PostgreSQL (T303; 05 8.5; 06 8.3.1; 09 13.2): the account is opened verified and without a role or a session (KK8), nothing reaches the
/// operation trail, the sent records or the job tables (KK9), the command's name is in its own log file before the work and carries no personal data (KK9), a refusal makes no
/// account and prints no value (KK6), and a real process with a redirected input ends with 2 and changes nothing (KK7).
/// </summary>
[Collection(CreateAdminCommandDefinition.Name)]
public sealed class CreateAdminCommandTests : IAsyncLifetime
{
    private const string Name = "Ayşe Yılmaz";
    private const string Email = "ayse.yilmaz@example.com";
    private const string StrongPassword = "orchid-lantern-4471-violet";

    // The tables the command must leave alone (KK9): the operation trail, the sent records, the job tables, the sign-in records and the sessions.
    private static readonly string[] UntouchedTables = ["audit_logs", "dispatch_records", "jobs", "login_records", "sessions"];

    private readonly CollectionDatabaseFixture _collection;
    private readonly string _logDirectory = Path.Combine(Path.GetTempPath(), "shopfolio-t303-" + Guid.NewGuid().ToString("N"));

    public CreateAdminCommandTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE admins, sessions, login_records, dispatch_records, jobs, audit_logs CASCADE");
    }

    public ValueTask DisposeAsync()
    {
        if (Directory.Exists(_logDirectory))
        {
            Directory.Delete(_logDirectory, recursive: true);
        }

        return ValueTask.CompletedTask;
    }

    // ---- KK8 and KK5: the account ----

    [Fact]
    public async Task CreateAdmin_OpensAVerifiedAccountWithTheGivenValues_AndExitsWithZero()
    {
        FakeTerminal terminal = Interactive(Name, Email, StrongPassword, StrongPassword);

        int exit = await RunAsync(terminal, "create-admin");

        Assert.Equal(0, exit);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(Name, await ScalarAsync<string>("SELECT name FROM admins"));
        Assert.Equal(Email, await ScalarAsync<string>("SELECT email FROM admins"));

        string digest = await ScalarAsync<string>("SELECT password_hash FROM admins");
        Assert.NotEqual(StrongPassword, digest);
        Assert.True(new PasswordService().Verify(digest, StrongPassword).Matched);
        Assert.False(new PasswordService().Verify(digest, StrongPassword + "x").Matched);

        await using AppDbContext context = NewContext();
        PasswordAccount? account = await new AdminPasswords(context).FindByEmailAsync(TextNormalizer.NormalizeIdentity(Email), Ct);
        Assert.NotNull(account);
        Assert.True(account.IsVerified);
        Assert.Equal(await ScalarAsync<Guid>("SELECT id FROM admins"), account.Id);
    }

    [Fact]
    public async Task CreateAdmin_AdminRowHasNoRoleColumn_AndNoSessionOrSignInRecordIsMade()
    {
        await RunAsync(Interactive(Name, Email, StrongPassword, StrongPassword), "create-admin");

        // 06 8.3.2: the administrator's row is the whole account; no column is a role or a state.
        string columns = await ScalarAsync<string>("SELECT string_agg(column_name, ',' ORDER BY column_name) FROM information_schema.columns WHERE table_name = 'admins'");
        Assert.DoesNotContain("role", columns, StringComparison.Ordinal);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM sessions"));
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM login_records"));
    }

    // ---- KK9: no trail, no record, no job ----

    [Fact]
    public async Task CreateAdmin_WritesNothingToTheTrailTheSentRecordsOrTheJobTables()
    {
        string before = await SnapshotAsync();

        int exit = await RunAsync(Interactive(Name, Email, StrongPassword, StrongPassword), "create-admin");

        Assert.Equal(0, exit);
        foreach (string table in UntouchedTables)
        {
            Assert.Equal(0L, await ScalarAsync<long>($"SELECT count(*) FROM {table}"));
        }

        Assert.Equal(before, await SnapshotAsync());
    }

    [Fact]
    public async Task CreateAdmin_CommandFile_HoldsOneLineWithTheCommandNameAndTheAccountId_AndNoPersonalValue()
    {
        int exit = await RunAsync(Interactive(Name, Email, StrongPassword, StrongPassword), "create-admin");
        Guid adminId = await ScalarAsync<Guid>("SELECT id FROM admins");

        string file = Assert.Single(Directory.GetFiles(_logDirectory, "command-*.jsonl"));
        Assert.Empty(Directory.GetFiles(_logDirectory, "audit-*.jsonl"));
        string text = ReadShared(file);
        string line = Assert.Single(text.Split('\n', StringSplitOptions.RemoveEmptyEntries));
        using JsonDocument json = JsonDocument.Parse(line);

        Assert.Equal(0, exit);
        Assert.Contains(LogEvents.AdminCommand, line, StringComparison.Ordinal);
        Assert.Contains("create-admin", line, StringComparison.Ordinal);
        Assert.Contains(adminId.ToString(), line, StringComparison.OrdinalIgnoreCase);
        Assert.True(json.RootElement.ValueKind == JsonValueKind.Object);
        Assert.DoesNotContain("Ayşe", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Yılmaz", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ayse", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("example.com", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(StrongPassword, text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAdmin_LineIsWrittenBeforeTheAccountExists_AndTheLineCarriesTheIdTheAccountGets()
    {
        FakeTerminal terminal = Interactive(Name, Email, StrongPassword, StrongPassword);
        List<(Guid Id, long Admins)> seen = [];
        string dsn = _collection.Dsn;

        int exit = await RunAsync(
            terminal,
            services => services.Replace(ServiceDescriptor.Singleton<IAuditLineWriter>(new SpyWriter(line =>
            {
                using NpgsqlConnection connection = new(dsn);
                connection.Open();
                using NpgsqlCommand command = new("SELECT count(*) FROM admins", connection);
                seen.Add((line.Id, (long)command.ExecuteScalar()!));
            }))),
            "create-admin");

        (Guid id, long adminsAtWriteTime) = Assert.Single(seen);
        Assert.Equal(0, exit);
        Assert.Equal(0L, adminsAtWriteTime);
        Assert.Equal(id, await ScalarAsync<Guid>("SELECT id FROM admins"));
    }

    [Fact]
    public async Task CreateAdmin_WhenTheLineCannotBeWritten_ExitsWithOneAndMakesNoAccount()
    {
        FakeTerminal terminal = Interactive(Name, Email, StrongPassword, StrongPassword);

        int exit = await RunAsync(
            terminal,
            services => services.Replace(ServiceDescriptor.Singleton<IAuditLineWriter>(new SpyWriter(_ => throw new IOException("the disk is full")))),
            "create-admin");

        Assert.Equal(1, exit);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Contains(nameof(IOException), terminal.Error, StringComparison.Ordinal);
        Assert.DoesNotContain(StrongPassword, terminal.AllOutput, StringComparison.Ordinal);
    }

    // ---- KK5 and KK6: refusals ----

    [Theory]
    [InlineData("short1", "ayse.yilmaz@example.com")]
    [InlineData("password1234", "ayse.yilmaz@example.com")]
    [InlineData("orchid-lantern-4471-violet", "not-an-address")]
    public async Task CreateAdmin_RefusedPasswordOrAddress_ExitsWithThree_MakesNoAccount_AndPrintsNoValue(string password, string email)
    {
        FakeTerminal terminal = Interactive(Name, email, password, password);

        int exit = await RunAsync(terminal, "create-admin");

        Assert.Equal(3, exit);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.DoesNotContain(password, terminal.AllOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(email, terminal.AllOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(Name, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAdmin_AnAddressThatHasAnAccount_ExitsWithThree_AndTheFirstAccountIsKept()
    {
        Assert.Equal(0, await RunAsync(Interactive(Name, Email, StrongPassword, StrongPassword), "create-admin"));
        string snapshot = await ScalarAsync<string>("SELECT to_jsonb(a)::text FROM admins a");
        FakeTerminal terminal = Interactive("Başka Biri", Email.ToUpperInvariant(), StrongPassword + "2", StrongPassword + "2");

        int exit = await RunAsync(terminal, "create-admin");

        Assert.Equal(3, exit);
        Assert.Equal(1L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.Equal(snapshot, await ScalarAsync<string>("SELECT to_jsonb(a)::text FROM admins a"));
        Assert.DoesNotContain(Email, terminal.AllOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Başka", terminal.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAdmin_EmptyNameAndTwoDifferentPasswords_AreRefusedBeforeTheDatabaseIsTouched()
    {
        FakeTerminal emptyName = Interactive("  ", Email, StrongPassword, StrongPassword);
        FakeTerminal different = Interactive(Name, Email, StrongPassword, StrongPassword + "x");

        Assert.Equal(3, await RunAsync(emptyName, "create-admin"));
        Assert.Equal(3, await RunAsync(different, "create-admin"));

        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.DoesNotContain(StrongPassword, emptyName.AllOutput + different.AllOutput, StringComparison.Ordinal);
    }

    // ---- KK2, KK3: a real process ----

    [Fact]
    public async Task RealProcess_WithARedirectedInput_ExitsWithTwo_WritesNoCommandFile_AndChangesNoRow()
    {
        string before = await SnapshotAsync();

        (int exit, string output) = await RunProcessAsync(["admin", "create-admin"], standardInput: Name + "\n" + Email + "\n" + StrongPassword + "\n" + StrongPassword + "\n");

        Assert.Equal(2, exit);
        Assert.Equal(before, await SnapshotAsync());
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
        Assert.True(!Directory.Exists(_logDirectory) || Directory.GetFiles(_logDirectory).Length == 0);
        Assert.DoesNotContain(StrongPassword, output, StringComparison.Ordinal);
        Assert.DoesNotContain(Email, output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RealProcess_WithAnUnknownCommand_ExitsWithTwoAtOnce_AndTheMessageHasNoArgumentValue()
    {
        (int exit, string output) = await RunProcessAsync(["admin", "bogus-secret-value"], standardInput: string.Empty);

        Assert.Equal(2, exit);
        Assert.DoesNotContain("bogus-secret-value", output, StringComparison.Ordinal);
        Assert.Equal(0L, await ScalarAsync<long>("SELECT count(*) FROM admins"));
    }

    [Fact]
    public async Task RealProcess_WithHelp_ExitsWithZero_AndListsTheCommand()
    {
        (int exit, string output) = await RunProcessAsync(["admin", "--help"], standardInput: string.Empty);

        Assert.Equal(0, exit);
        Assert.Contains("create-admin", output, StringComparison.Ordinal);
    }

    // ---- helpers ----

    private static FakeTerminal Interactive(string name, string email, string password, string repeated)
    {
        return new FakeTerminal { IsInputRedirected = false }.Answer(name).Answer(email).Secret(password).Secret(repeated);
    }

    private static string ReadShared(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private Task<int> RunAsync(FakeTerminal terminal, params string[] arguments) => RunAsync(terminal, _ => { }, arguments);

    private async Task<int> RunAsync(FakeTerminal terminal, Action<IServiceCollection> adjust, params string[] arguments)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = _collection.Dsn,
                [LoggingOptions.DirectoryKey] = _logDirectory,
            })
            .Build();
        IServiceCollection services = CommandHost.CreateServices(configuration, terminal);
        adjust(services);

        CommandDispatcher dispatcher = new(AdminCommandCatalog.All, terminal, () => services.BuildServiceProvider());
        return await dispatcher.ExecuteAsync(arguments, Ct);
    }

    private async Task<(int Exit, string Output)> RunProcessAsync(string[] arguments, string standardInput)
    {
        string application = Path.Combine(AppContext.BaseDirectory, "Shopfolio.Web.dll");
        ProcessStartInfo start = new("dotnet")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = AppContext.BaseDirectory,
        };
        start.ArgumentList.Add(application);
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment["Database__ConnectionString"] = _collection.Dsn;
        start.Environment[LoggingOptions.DirectoryKey.Replace(":", "__", StringComparison.Ordinal)] = _logDirectory;

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("The process did not start.");
        await process.StandardInput.WriteAsync(standardInput.AsMemory(), Ct);
        process.StandardInput.Close();

        Task<string> output = process.StandardOutput.ReadToEndAsync(Ct);
        Task<string> error = process.StandardError.ReadToEndAsync(Ct);
        try
        {
            await process.WaitForExitAsync(Ct).WaitAsync(TimeSpan.FromSeconds(90), Ct);
        }
        catch (Exception exception) when (exception is TimeoutException or OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        return (process.ExitCode, await output + await error);
    }

    private async Task<string> SnapshotAsync()
    {
        List<string> parts = [];
        foreach (string table in UntouchedTables)
        {
            parts.Add(table + ":" + await ScalarAsync<string>($"SELECT coalesce(string_agg(t.xmin::text || to_jsonb(t)::text, '|'), '') FROM {table} t"));
        }

        return string.Join('\n', parts);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private sealed class SpyWriter : IAuditLineWriter
    {
        private readonly Action<AuditLine> _onWrite;

        public SpyWriter(Action<AuditLine> onWrite)
        {
            _onWrite = onWrite;
        }

        public void Write(AuditLine line) => _onWrite(line);
    }
}
