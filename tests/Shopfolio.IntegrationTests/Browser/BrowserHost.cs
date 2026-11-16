using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Playwright;

using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>The collection of the browser tests (T551): one database copied from the template, one application behind a real socket, one Playwright.</summary>
[CollectionDefinition(Name)]
public sealed class BrowserTestsDefinition : ICollectionFixture<BrowserHost>
{
    public const string Name = "BrowserTests";
}

/// <summary>Which of the two ways the page is opened in (12 4.17: the script on, the script off).</summary>
public enum ScriptPath
{
    /// <summary>The page's script runs (the scripted way).</summary>
    Scripted,

    /// <summary>The browser context has the script switched off (the scriptless way, 07 8.2.1).</summary>
    Scriptless,
}

/// <summary>
/// The tool of the browser observation (T551; 12 4.17, 12.2; K-1620): the application behind a real Kestrel socket on the collection's database, the three
/// engines of Playwright (Chromium, Firefox, WebKit) and the contexts a page is opened in - one with the script on and one with the script off. The
/// run's versions, the seed and every observation are written to the record the gate report attaches (<see cref="Record"/>).
/// </summary>
public sealed class BrowserHost : IAsyncLifetime
{
    /// <summary>The three engines the tool runs on (12 4.17 "Tarayıcı kümesi": the three engines' representatives).</summary>
    public static readonly string[] Engines = EnginesOfTheRun();

    private static string[] EnginesOfTheRun()
    {
        string[] all = ["chromium", "firefox", "webkit"];
        string? chosen = Environment.GetEnvironmentVariable("SHOPFOLIO_BROWSER_ENGINES");
        return string.IsNullOrWhiteSpace(chosen) ? all : [.. all.Where(e => chosen.Split(',').Contains(e))];
    }

    /// <summary>The widths the observations are taken at: a desktop and a phone (12 4.17 "genişlik").</summary>
    public const int DesktopWidth = 1280;

    private readonly TemplateDatabaseFixture _template;
    private string _databaseName = string.Empty;
    private readonly Dictionary<string, IBrowser> _browsers = [];
    private ShopfolioWebApplicationFactory? _root;
    private WebApplicationFactory<Program>? _factory;
    private IPlaywright? _playwright;

    public BrowserHost(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    /// <summary>The address the application listens on (a real loopback socket).</summary>
    public string BaseUrl { get; private set; } = string.Empty;

    /// <summary>The connection string of the collection's database (the rows' snapshots read it).</summary>
    public string Dsn { get; private set; } = string.Empty;

    /// <summary>The seeded data every observation starts from (<see cref="BrowserSeed"/>).</summary>
    public BrowserSeedData Seed { get; private set; } = null!;

    /// <summary>The record of the run: observations, versions and the seed.</summary>
    public ObservationRecord Record { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _databaseName = await _template.CreateDatabaseFromTemplateAsync(CancellationToken.None);
        Dsn = _template.DsnFor(_databaseName);
        // The sales gate is open for the run (the gate's own conditions are the integration tests' subject, 07 8.2.1); everything else is the application as it is.
        _root = new ShopfolioWebApplicationFactory(Dsn);
        _factory = _root.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            // The browser connects straight to the application: the tool plays the reverse proxy, so the loopback is trusted for the forwarded headers (the application trusts the proxy network only, 09 15.1 item 8).
            s.Configure<ForwardedHeadersOptions>(o => o.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("127.0.0.0/8")));
            s.RemoveAll<ISettingsModule>();
            s.AddScoped(sp => CheckoutPageTests.GateSettings.Create(ActivatorUtilities.CreateInstance<SettingsModule>(sp), true));
        }));
        _factory.Start();
        using HttpClient probe = _factory.CreateClient();
        // The browsers take the name localhost as a secure origin: a Secure cookie (and the __Host- prefix) is accepted and sent there over plain HTTP.
        BaseUrl = $"http://localhost:{probe.BaseAddress!.Port}";
        _playwright = await Playwright.CreateAsync();
        Seed = await BrowserSeed.RunAsync(Dsn);
        Record = new ObservationRecord(OutputDirectory());
        Record.SetSeed(Seed);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (IBrowser browser in _browsers.Values)
        {
            await browser.CloseAsync();
        }

        _playwright?.Dispose();
        KeepServerLog();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        Record?.Flush();
        if (_databaseName.Length > 0)
        {
            using Npgsql.NpgsqlConnection pooled = new(Dsn);
            Npgsql.NpgsqlConnection.ClearPool(pooled);
            await _template.DropDatabaseAsync(_databaseName, CancellationToken.None);
        }
    }

    /// <summary>Copies the application's own log files beside the record (the factory deletes its directory when it is disposed).</summary>
    private void KeepServerLog()
    {
        if (_root is null || !Directory.Exists(_root.LogDirectory))
        {
            return;
        }

        string target = System.IO.Path.Combine(Record.Directory, "server-log");
        Directory.CreateDirectory(target);
        foreach (string file in Directory.GetFiles(_root.LogDirectory))
        {
            using FileStream source = new(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using FileStream copy = File.Create(System.IO.Path.Combine(target, System.IO.Path.GetFileName(file)));
            source.CopyTo(copy);
        }
    }

    /// <summary>The engine's browser; launched once, its version goes to the record.</summary>
    public async Task<IBrowser> BrowserAsync(string engine)
    {
        if (_browsers.TryGetValue(engine, out IBrowser? found))
        {
            return found;
        }

        IBrowserType type = engine switch
        {
            "chromium" => _playwright!.Chromium,
            "firefox" => _playwright!.Firefox,
            "webkit" => _playwright!.Webkit,
            _ => throw new ArgumentOutOfRangeException(nameof(engine), engine, "Unknown engine."),
        };
        IBrowser browser = await type.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _browsers[engine] = browser;
        Record.SetEngineVersion(engine, browser.Version);
        return browser;
    }

    /// <summary>
    /// A fresh context of the engine for the way: with the script on or off, the application's own proxy header (the application takes its scheme from it, 09 15.1 item 8) and,
    /// for a signed-in actor, the session cookie of the seeded account.
    /// </summary>
    public async Task<IBrowserContext> ContextAsync(string engine, ScriptPath path, Actor actor = Actor.Visitor, int width = DesktopWidth, string? cartKey = null)
    {
        IBrowser browser = await BrowserAsync(engine);
        IBrowserContext context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            JavaScriptEnabled = path == ScriptPath.Scripted,
            ViewportSize = new ViewportSize { Width = width, Height = width < 600 ? 800 : 900 },
            ExtraHTTPHeaders = new Dictionary<string, string> { ["X-Forwarded-Proto"] = "https", ["X-Forwarded-For"] = "203.0.113.9" },
            Locale = "tr-TR",
        });
        List<Cookie> cookies = [];
        if (actor == Actor.Customer)
        {
            cookies.Add(Secure(CookieNames.Session, Seed.CustomerKey));
        }

        if (actor == Actor.Admin)
        {
            cookies.Add(Secure(CookieNames.AdminSession, Seed.AdminKey));
        }

        if (cartKey is not null)
        {
            cookies.Add(Secure(CookieNames.Cart, cartKey));
        }

        if (cookies.Count > 0)
        {
            await context.AddCookiesAsync(cookies);
        }

        return context;
    }

    private static Cookie Secure(string name, string value) => new()
    {
        Name = name,
        Value = value,
        Url = "https://localhost",
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteAttribute.Lax,
    };

    private static string OutputDirectory()
    {
        string? configured = Environment.GetEnvironmentVariable("SHOPFOLIO_BROWSER_OUT");
        string directory = string.IsNullOrWhiteSpace(configured) ? Path.Combine(Path.GetTempPath(), "shopfolio-browser-observations") : configured;
        Directory.CreateDirectory(directory);
        return directory;
    }
}

/// <summary>Who the page is opened as.</summary>
public enum Actor
{
    Visitor,
    Customer,
    Admin,
}

/// <summary>One observation (12 7.1 "Tarayıcı gözlemi"): the row, the way, the engine and its version, the width, the step, what was expected and what was seen, the screenshot.</summary>
public sealed record Observation(string Row, string Path, string Engine, int Width, string Step, string Expected, string Observed, bool Ok, string? Screenshot);

/// <summary>The record the gate report attaches (12 12.2 (7)): versions, the seed, and one line per observation. Written as JSON lines and a Markdown table.</summary>
public sealed class ObservationRecord
{
    private readonly string _directory;
    private readonly object _lock = new();
    private readonly List<Observation> _observations = [];
    private readonly Dictionary<string, string> _versions = [];
    private string _seed = "{}";

    public ObservationRecord(string directory)
    {
        _directory = directory;
        _versions["playwright"] = typeof(Playwright).Assembly.GetName().Version?.ToString() ?? "?";
        _versions["dotnet"] = Environment.Version.ToString();
    }

    public string Directory => _directory;

    public void SetSeed(BrowserSeedData seed)
    {
        _seed = JsonSerializer.Serialize(seed.ForRecord());
    }

    public void SetEngineVersion(string engine, string version)
    {
        lock (_lock)
        {
            _versions[engine] = version;
        }
    }

    public void Add(Observation observation)
    {
        lock (_lock)
        {
            _observations.Add(observation);
        }
    }

    /// <summary>Writes <c>observations.jsonl</c> and <c>observations.md</c>; the files of earlier runs are replaced.</summary>
    public void Flush()
    {
        lock (_lock)
        {
            JsonSerializerOptions options = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            using StreamWriter lines = new(System.IO.Path.Combine(_directory, "observations.jsonl"), append: false);
            lines.WriteLine(JsonSerializer.Serialize(new { versions = _versions, seed = JsonDocument.Parse(_seed).RootElement }, options));
            foreach (Observation observation in _observations)
            {
                lines.WriteLine(JsonSerializer.Serialize(observation, options));
            }

            using StreamWriter table = new(System.IO.Path.Combine(_directory, "observations.md"), append: false);
            table.WriteLine("# Tarayıcı gözlem kaydı (T551)");
            table.WriteLine();
            table.WriteLine("Sürümler: " + string.Join(", ", _versions.Select(v => $"{v.Key} {v.Value}")));
            table.WriteLine();
            table.WriteLine("Tohum: " + _seed);
            table.WriteLine();
            table.WriteLine("| Satır | Yol | Tarayıcı | Genişlik | Adım | Beklenen | Gözlenen | Sonuç | Ekran görüntüsü |");
            table.WriteLine("|---|---|---|---|---|---|---|---|---|");
            foreach (Observation o in _observations.OrderBy(o => o.Row, StringComparer.Ordinal).ThenBy(o => o.Engine, StringComparer.Ordinal).ThenBy(o => o.Path, StringComparer.Ordinal))
            {
                table.WriteLine($"| {o.Row} | {o.Path} | {o.Engine} | {o.Width} | {Cell(o.Step)} | {Cell(o.Expected)} | {Cell(o.Observed)} | {(o.Ok ? "✓" : "✗")} | {o.Screenshot} |");
            }
        }
    }

    private static string Cell(string text) => text.Replace("|", "\\|", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
}
