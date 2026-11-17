using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
    private readonly System.Runtime.CompilerServices.StrongBox<bool> _cardOpen = new(false);
    private string _databaseName = string.Empty;
    private readonly Dictionary<string, IBrowser> _browsers = [];
    private ShopfolioWebApplicationFactory? _root;
    private WebApplicationFactory<Program>? _factory;
    private IPlaywright? _playwright;

    private readonly bool _empty;
    private readonly SemaphoreSlim _companionLock = new(1, 1);
    private BrowserHost? _companion;

    public BrowserHost(TemplateDatabaseFixture template)
    {
        _template = template;
    }

    private BrowserHost(TemplateDatabaseFixture template, bool empty)
    {
        _template = template;
        _empty = empty;
    }

    /// <summary>
    /// The host of an installation with no record at all (KK2 (b): the empty state of the panel's lists): its own database made from the template, its own application, one administrator who can sign in and nothing else.
    /// It is made once on the first ask and goes away with this host; its observations are kept in this host's record (it keeps none of its own).
    /// </summary>
    public async Task<BrowserHost> EmptyAsync()
    {
        await _companionLock.WaitAsync();
        try
        {
            if (_companion is null)
            {
                BrowserHost made = new(_template, empty: true);
                await made.InitializeAsync();
                _companion = made;
            }

            return _companion;
        }
        finally
        {
            _companionLock.Release();
        }
    }

    /// <summary>The address the application listens on (a real loopback socket).</summary>
    public string BaseUrl { get; private set; } = string.Empty;

    /// <summary>The connection string of the collection's database (the rows' snapshots read it).</summary>
    public string Dsn { get; private set; } = string.Empty;

    /// <summary>The seeded data every observation starts from (<see cref="BrowserSeed"/>).</summary>
    public BrowserSeedData Seed { get; private set; } = null!;

    /// <summary>The address key of a valid administrator invitation (the invitation screen's form opens for such a link only); never written to the record.</summary>
    public string InvitationKey { get; private set; } = string.Empty;

    /// <summary>The address keys of one valid link of every kind behind the e-mailed link screens (the walk opens each screen in its valid-link state); never written to the record.</summary>
    internal BrowserSeed.SeededLinks Links { get; private set; } = null!;

    /// <summary>The application's own link keys: a flow that asks for a link (the reset request, the registration) derives the key the e-mail would carry from the link row.</summary>
    internal Shopfolio.Infrastructure.Security.LinkKeys LinkKeys => _factory!.Services.GetRequiredService<Shopfolio.Infrastructure.Security.LinkKeys>();

    /// <summary>The record of the run: observations, versions and the seed.</summary>
    public ObservationRecord Record { get; private set; } = null!;

    /// <summary>Opens the card line of the conditional settings (the payment provider's keys defined) until the returned scope is disposed: the payment step then offers two open methods. The tests of a collection run one after the other, so one switch is enough.</summary>
    public IDisposable OpenCard()
    {
        _cardOpen.Value = true;
        return new CardScope(_cardOpen);
    }

    private sealed class CardScope(System.Runtime.CompilerServices.StrongBox<bool> box) : IDisposable
    {
        public void Dispose() => box.Value = false;
    }

    /// <summary>The conditional lines of the installation with the card line forced open while the run's switch is on.</summary>
    private sealed class SwitchableLines(IReadOnlyList<Shopfolio.Infrastructure.InstallationSettings.ConditionalLine> installed, System.Runtime.CompilerServices.StrongBox<bool> cardOpen) : IReadOnlyList<Shopfolio.Infrastructure.InstallationSettings.ConditionalLine>
    {
        public int Count => installed.Count;

        public Shopfolio.Infrastructure.InstallationSettings.ConditionalLine this[int index] => Of(installed[index]);

        public IEnumerator<Shopfolio.Infrastructure.InstallationSettings.ConditionalLine> GetEnumerator() => installed.Select(Of).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        private Shopfolio.Infrastructure.InstallationSettings.ConditionalLine Of(Shopfolio.Infrastructure.InstallationSettings.ConditionalLine line) =>
            cardOpen.Value && line.Name == Shopfolio.Infrastructure.InstallationSettings.ConditionalLine.CardPayment ? line with { MissingKeys = [] } : line;
    }

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
            // The upload sections of the file store are made-up directories of the run (the default ones are the container volumes, which a Windows host does not have).
            s.RemoveAll<Shopfolio.Infrastructure.Files.FileStoreDirectories>();
            s.AddSingleton(new Shopfolio.Infrastructure.Files.FileStoreDirectories(System.IO.Path.Combine(_filesDirectory, "images"), System.IO.Path.Combine(_filesDirectory, "digital")));
            s.AddScoped(sp => CheckoutPageTests.GateSettings.Create(ActivatorUtilities.CreateInstance<SettingsModule>(sp), true));
            // The card line of the conditional settings follows the run's switch (OpenCard): closed as the installation has it, open for the one observation that needs a second open payment method (VAL-B099).
            ServiceDescriptor original = s.Last(d => d.ServiceType == typeof(IReadOnlyList<Shopfolio.Infrastructure.InstallationSettings.ConditionalLine>));
            s.Remove(original);
            s.AddSingleton<IReadOnlyList<Shopfolio.Infrastructure.InstallationSettings.ConditionalLine>>(sp => new SwitchableLines(
                (IReadOnlyList<Shopfolio.Infrastructure.InstallationSettings.ConditionalLine>)(original.ImplementationFactory?.Invoke(sp) ?? original.ImplementationInstance!),
                _cardOpen));
        }));
        // The application is served over HTTPS with a throwaway certificate of the tool (the contexts ignore its trust): WebKit does not send a Secure cookie over plain HTTP even to localhost, so every engine gets the same, real scheme.
        _certificate = NewCertificate();
        _factory.UseKestrel(options => options.Listen(System.Net.IPAddress.Loopback, 0, listen => listen.UseHttps(_certificate)));
        _factory.StartServer();
        using HttpClient probe = _factory.CreateClient();
        BaseUrl = $"https://127.0.0.1:{probe.BaseAddress!.Port}";
        _playwright = await Playwright.CreateAsync();
        if (_empty)
        {
            (string adminKey, Guid adminId) = await TestSessions.AdminAsync(Dsn, longLived: true);
            Seed = new BrowserSeedData { AdminKey = adminKey, AdminId = adminId };
            return;
        }

        Seed = await BrowserSeed.RunAsync(Dsn, System.IO.Path.Combine(_filesDirectory, "images"));
        InvitationKey = await BrowserSeed.NewInvitationKeyAsync(Dsn, LinkKeys);
        Links = await BrowserSeed.NewLinksAsync(Dsn, LinkKeys);
        Record = new ObservationRecord(OutputDirectory());
        Record.SetSeed(Seed);
    }

    public async ValueTask DisposeAsync()
    {
        if (_companion is not null)
        {
            await _companion.DisposeAsync();
        }

        _companionLock.Dispose();
        foreach (IBrowser browser in _browsers.Values)
        {
            await browser.CloseAsync();
        }

        _playwright?.Dispose();
        if (!_empty)
        {
            KeepServerLog();
        }

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        _certificate?.Dispose();
        try
        {
            Directory.Delete(_filesDirectory, recursive: true);
        }
        catch (IOException)
        {
            // The run's upload files are in the temporary directory; what cannot be removed now is removed with it.
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
        Record?.SetEngineVersion(engine, browser.Version);
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
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = width, Height = width < 600 ? 800 : 900 },
            ExtraHTTPHeaders = new Dictionary<string, string> { ["X-Forwarded-Proto"] = "https", ["X-Forwarded-For"] = NextClientAddress() },
            Locale = "tr-TR",
        });
        List<Cookie> cookies = [];
        if (actor == Actor.Customer)
        {
            cookies.Add(Secure(CookieNames.Session, Seed.CustomerKey));
        }

        if (actor == Actor.BareCustomer)
        {
            cookies.Add(Secure(CookieNames.Session, Seed.BareCustomerKey));
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

    private Cookie Secure(string name, string value) => new()
    {
        Name = name,
        Value = value,
        Url = BaseUrl,
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteAttribute.Lax,
    };

    private readonly string _filesDirectory = Path.Combine(Path.GetTempPath(), "t551-browser-files-" + Guid.NewGuid().ToString("N"));
    private int _clients;
    private System.Security.Cryptography.X509Certificates.X509Certificate2? _certificate;

    /// <summary>A client address of its own for every context: the order limits count the open unpaid orders by address (L-8), so a run that places many orders must not share one.</summary>
    private string NextClientAddress()
    {
        int n = Interlocked.Increment(ref _clients);
        return $"198.51.{n / 250 % 250}.{(n % 250) + 1}";
    }

    private static System.Security.Cryptography.X509Certificates.X509Certificate2 NewCertificate()
    {
        using System.Security.Cryptography.RSA key = System.Security.Cryptography.RSA.Create(2048);
        System.Security.Cryptography.X509Certificates.CertificateRequest request = new("CN=localhost", key, System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder names = new();
        names.AddDnsName("localhost");
        names.AddIpAddress(System.Net.IPAddress.Loopback);
        names.AddIpAddress(System.Net.IPAddress.IPv6Loopback);
        request.CertificateExtensions.Add(names.Build());
        request.CertificateExtensions.Add(new System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension([new System.Security.Cryptography.Oid("1.3.6.1.5.5.7.3.1")], false));
        using System.Security.Cryptography.X509Certificates.X509Certificate2 created = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(2));
        // Exported and loaded again so the private key is usable by the server on every platform.
        return System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12(created.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx), null);
    }

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

    /// <summary>A member with no address and no order (<see cref="BrowserSeedData.BareCustomerKey"/>): the actor of the empty states of the member's screens.</summary>
    BareCustomer,
    Admin,
}

/// <summary>One observation (12 7.1 "Tarayıcı gözlemi"): the row, the way, the engine and its version, the width, the step, what was expected and what was seen, the screenshot.</summary>
public sealed record Observation(string Row, string Path, string Engine, int Width, string Step, string Expected, string Observed, bool Ok, string? Screenshot, string? Data = null);

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

    /// <summary>The observations added so far; a test reads them to prove what the record keeps (the masked text), not what the page showed.</summary>
    public IReadOnlyList<Observation> Snapshot()
    {
        lock (_lock)
        {
            return [.. _observations];
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
            table.WriteLine("| Satır | Yol | Tarayıcı | Genişlik | Adım | Beklenen | Gözlenen | Veri kaydı (maskeli) | Sonuç | Ekran görüntüsü |");
            table.WriteLine("|---|---|---|---|---|---|---|---|---|---|");
            foreach (Observation o in _observations.OrderBy(o => o.Row, StringComparer.Ordinal).ThenBy(o => o.Engine, StringComparer.Ordinal).ThenBy(o => o.Path, StringComparer.Ordinal))
            {
                table.WriteLine($"| {o.Row} | {o.Path} | {o.Engine} | {o.Width} | {Cell(o.Step)} | {Cell(o.Expected)} | {Cell(o.Observed)} | {Cell(o.Data ?? "-")} | {(o.Ok ? "✓" : "✗")} | {o.Screenshot} |");
            }
        }
    }

    private static string Cell(string text) => text.Replace("|", "\\|", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
}
