using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Shopfolio.Commands;
using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.UnitTests.Architecture;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Commands;

/// <summary>
/// The shape of the command mode (T303; 05 8.5.1; 09 13.2 items 1, 2, 3, 6; KK1, KK3, KK4): the service collection has no hosted service and no server service, the database context
/// carries the maintenance session times, the catalog is the one static list whose every command is a row of the <c>05 8.5</c> table, and the source of the folder keeps to the terminal
/// abstraction (the scan is shown red on a sample of each form).
/// </summary>
public sealed partial class CommandStructureTests
{
    private const string ConnectionKey = "Database:ConnectionString";

    private static readonly string[] _serverTypeNames = ["IServer", "GenericWebHostService", "KestrelServer", "IWebHost", "WebApplication"];

    private static IConfiguration Configuration()
    {
        // A made-up connection string that belongs to no installation; the test never opens a connection.
        return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { [ConnectionKey] = "Host=127.0.0.1;Database=unused;Username=unused" }).Build();
    }

    // --- KK1: no hosted service, no server ---

    [Fact]
    public void CommandModeServices_HaveNoHostedServiceAndNoServerService()
    {
        IServiceCollection services = CommandHost.CreateServices(Configuration(), new FakeTerminal());

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IHostedService));
        Assert.DoesNotContain(services, d => d.ServiceType.IsAssignableTo(typeof(IHostedService)));
        Assert.DoesNotContain(services, d => d.ImplementationType is not null && d.ImplementationType.IsAssignableTo(typeof(IHostedService)));
        Assert.DoesNotContain(services, d => _serverTypeNames.Contains(d.ServiceType.Name, StringComparer.Ordinal)
            || (d.ImplementationType is not null && _serverTypeNames.Contains(d.ImplementationType.Name, StringComparer.Ordinal)));

        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Empty(provider.GetServices<IHostedService>());
    }

    [Fact]
    public void CommandModeServices_ResolveTheAdministrationModuleAndTheTransactionRunner()
    {
        IServiceCollection services = CommandHost.CreateServices(Configuration(), new FakeTerminal());
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using IServiceScope scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAdministrationModule>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ITransactionRunner>());
    }

    // --- KK4: the maintenance profile ---

    [Fact]
    public void CommandModeContext_UsesTheMaintenanceSessionTimes_NotTheWebProfiles()
    {
        IServiceCollection services = CommandHost.CreateServices(Configuration(), new FakeTerminal());
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        string? used = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.GetConnectionString();

        string source = Configuration()[ConnectionKey]!;
        Assert.Equal(DatabaseConnection.Create(source, DatabaseSessionProfile.Maintenance), used);
        Assert.NotEqual(DatabaseConnection.Create(source, DatabaseSessionProfile.Web), used);
    }

    // --- Mode selection ---

    [Theory]
    [InlineData("admin", true)]
    [InlineData("Admin", false)]
    [InlineData("administrator", false)]
    [InlineData("--urls", false)]
    public void CommandMode_IsSelectedByTheFirstArgumentAdminOnly(string first, bool expected)
    {
        Assert.Equal(expected, CommandHost.IsCommandMode([first, "create-admin"]));
        Assert.False(CommandHost.IsCommandMode([]));
        Assert.False(CommandHost.IsCommandMode(["create-admin", first]));
    }

    // --- KK3: the catalog and the 05 8.5 table ---

    [Fact]
    public void Catalog_HasUniqueKebabCaseNamesAndOneLineDescriptions()
    {
        IReadOnlyList<IAdminCommand> all = AdminCommandCatalog.All;

        Assert.NotEmpty(all);
        Assert.Equal(all.Count, all.Select(c => c.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.All(all, c => Assert.Matches(KebabCase(), c.Name));
        Assert.All(all, c => Assert.False(string.IsNullOrWhiteSpace(c.Description) || c.Description.Contains('\n', StringComparison.Ordinal)));
        Assert.All(all, c => Assert.Equal(c.Options.Count, c.Options.Select(o => o.Name).Distinct(StringComparer.Ordinal).Count()));
    }

    [Fact]
    public void EveryListedCommand_IsARowOfThe09TableAndOfThe05Table()
    {
        string root = RepositoryRoot.Find();
        Dictionary<string, string> rowOfCommand = CommandRows(File.ReadAllText(Path.Combine(root, "Docs", "09_CODING_GUIDELINES.md")));
        string architecture = File.ReadAllText(Path.Combine(root, "Docs", "05_TECHNICAL_ARCHITECTURE.md"));

        Assert.NotEmpty(rowOfCommand);
        foreach (IAdminCommand command in AdminCommandCatalog.All)
        {
            Assert.True(rowOfCommand.TryGetValue(command.Name, out string? row), $"{command.Name} is not a row of the 09 13.2 table");
            Assert.Matches(new Regex($@"^\| {Regex.Escape(row!)} \| [^|]+ \|", RegexOptions.Multiline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), architecture);
        }
    }

    [Fact]
    public void ListedOptions_AreTheOnesThe09TableNames()
    {
        string table = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md"));

        foreach (IAdminCommand command in AdminCommandCatalog.All)
        {
            Match row = Regex.Match(table, $@"^\s*\| `{Regex.Escape(command.Name)}` \| [^|]+ \| (?<options>[^\n]+)$", RegexOptions.Multiline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
            Assert.True(row.Success, command.Name);
            foreach (CommandOption option in command.Options)
            {
                Assert.Contains($"`--{option.Name}`", row.Groups["options"].Value, StringComparison.Ordinal);
            }

            if (command.Options.Count == 0)
            {
                Assert.StartsWith("yok", row.Groups["options"].Value.Trim().TrimStart('|').Trim(), StringComparison.Ordinal);
            }
        }
    }

    private static Dictionary<string, string> CommandRows(string guidelines)
    {
        Dictionary<string, string> rows = new(StringComparer.Ordinal);
        foreach (Match match in CommandTableRow().Matches(guidelines))
        {
            rows[match.Groups["name"].Value] = match.Groups["row"].Value;
        }

        return rows;
    }

    [GeneratedRegex(@"^\s*\| `(?<name>[a-z]+(?:-[a-z]+)*)` \| (?<row>8\.5\.\d+) \|", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CommandTableRow();

    [GeneratedRegex(@"^[a-z]+(-[a-z]+)*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex KebabCase();

    // --- The source scan of the Commands folder ---

    [Fact]
    public void CommandsSource_KeepsToTheTerminalTheContractsAndTheHost()
    {
        string root = RepositoryRoot.Find();
        string[] files = Directory.GetFiles(Path.Combine(root, "src", "Shopfolio.Web", "Commands"), "*.cs", SearchOption.AllDirectories);
        List<string> findings = [];
        foreach (string file in files)
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            Assert.True(CommandSourceScan.Applies(relative));
            findings.AddRange(CommandSourceScan.Scan(relative, File.ReadAllText(file)).Select(f => $"{relative}:{f.Line} {f.Rule}"));
        }

        Assert.Contains(files, f => f.EndsWith("Terminal.cs", StringComparison.Ordinal));
        Assert.Contains(files, f => f.EndsWith("CommandHost.cs", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Fact]
    public void CommandsScan_OnViolationSamples_ReportsEachFormAndSparesTheAllowedPlaces()
    {
        const string command = "src/Shopfolio.Web/Commands/SomeCommand.cs";
        const string terminal = "src/Shopfolio.Web/Commands/Terminal.cs";
        const string host = "src/Shopfolio.Web/Commands/CommandHost.cs";

        Assert.Equal([(CommandSourceScan.ConsoleOutsideTerminal, 2)], CommandSourceScan.Scan(command, "// Console.WriteLine in a comment\nConsole.WriteLine(x);\n"));
        Assert.Empty(CommandSourceScan.Scan(terminal, "Console.WriteLine(x);\nConsole.ReadKey(intercept: true);\n"));
        Assert.Equal([(CommandSourceScan.KeyWithEcho, 1)], CommandSourceScan.Scan(terminal, "Console.ReadKey();\n"));
        Assert.Equal([(CommandSourceScan.KeyWithEcho, 1)], CommandSourceScan.Scan(terminal, "Console.ReadKey(false);\n"));
        Assert.Equal([(CommandSourceScan.ContextInCommand, 1)], CommandSourceScan.Scan(command, "AppDbContext db = null;\n"));
        Assert.Empty(CommandSourceScan.Scan(host, "services.AddDbContext<AppDbContext>();\n"));
        Assert.Equal([(CommandSourceScan.EnvironmentOrCommandLine, 1)], CommandSourceScan.Scan(command, "Environment.GetEnvironmentVariable(\"X\");\n"));
        Assert.Equal([(CommandSourceScan.EnvironmentOrCommandLine, 1)], CommandSourceScan.Scan(command, "Environment.GetCommandLineArgs();\n"));
        Assert.False(CommandSourceScan.Applies("src/Shopfolio.Web/Infrastructure/Jobs/JobRunnerService.cs"));
    }
}
