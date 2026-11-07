using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Status;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>The collection of the file endpoint tests (T312): one database copied from the template.</summary>
[CollectionDefinition(FileEndpointsDefinition.Name)]
public sealed class FileEndpointsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "FileEndpoints";
}

/// <summary>
/// The opened application of the file endpoint tests (T312) on the collection database: a made-up file store in a temporary directory, a clock the test moves
/// (it starts at the real time, because the sessions of the test accounts are made with it) and a disk with plenty of room (a test asks for a full one).
/// The client sends the administrator's session cookie when it is given one and puts the forgery token on its writing requests (<see cref="ForgeryTokenHandler"/>).
/// </summary>
internal sealed class FileEndpointsHost : IAsyncDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "shopfolio-t312-" + Guid.NewGuid().ToString("N"));
    private readonly ShopfolioWebApplicationFactory _baseFactory;
    private readonly WebApplicationFactory<Program> _factory;

    public FileEndpointsHost(string dsn, bool kestrel = false, string? adminKey = null)
    {
        Dsn = dsn;
        Time = new ManualTimeProvider(DateTimeOffset.UtcNow);
        _baseFactory = new ShopfolioWebApplicationFactory(dsn, withJobRunner: false);
        _factory = _baseFactory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);
            services.RemoveAll<FileStoreDirectories>();
            services.AddSingleton(new FileStoreDirectories(Path.Combine(_root, "images"), Path.Combine(_root, "digital")));
            services.RemoveAll<IDiskUsageReader>();
            services.AddSingleton<IDiskUsageReader>(Disk);
            services.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly);
            if (kestrel)
            {
                // The real server sees the test client as a peer on the loopback; the test client plays the reverse proxy that says the request came over HTTPS.
                services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
                {
                    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("127.0.0.0/8"));
                    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("::1/128"));
                });
            }
        }));

        if (kestrel)
        {
            _factory.UseKestrel(0);
            _factory.StartServer();
            using HttpClient probe = _factory.CreateClient();
            Client = new HttpClient(new ForgeryTokenHandler { InnerHandler = new ForwardedHttpsHandler { InnerHandler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false } } })
            {
                BaseAddress = probe.BaseAddress,
            };
        }
        else
        {
            Client = ForgeryTokenHandler.CreateClient(_factory);
        }

        if (adminKey is not null)
        {
            Client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={adminKey}");
        }
    }

    public string Dsn { get; }

    public ManualTimeProvider Time { get; }

    public FakeDisk Disk { get; } = new();

    public HttpClient Client { get; }

    public IFileStore Files => _factory.Services.GetRequiredService<IFileStore>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _baseFactory.DisposeAsync();
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
            // A temporary directory that cannot be removed is left to the operating system.
        }
    }

    public AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, Dsn);
        return new AppDbContext(builder.Options);
    }

    public string LinkFor(Guid orderItemId) => _factory.Services.GetRequiredService<DownloadLinks>().Create(orderItemId, Time);

    // --- Upload requests ---

    public async Task<HttpResponseMessage> StartAsync(string json)
    {
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        return await Client.PostAsync(new Uri("/panel/yukleme", UriKind.Relative), content, Ct);
    }

    public async Task<Guid> StartOkAsync(string name, long size)
    {
        using HttpResponseMessage response = await StartAsync($"{{\"fileName\":\"{name}\",\"sizeBytes\":{size}}}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        return body.RootElement.GetProperty("id").GetGuid();
    }

    public async Task<HttpResponseMessage> ChunkAsync(string id, byte[] bytes, long start, bool noToken = false)
    {
        using ByteArrayContent content = new(bytes);
        content.Headers.ContentRange = new ContentRangeHeaderValue(start, start + bytes.Length - 1);
        return await SendChunkAsync(id, content, noToken);
    }

    public async Task<HttpResponseMessage> SendChunkAsync(string id, HttpContent content, bool noToken = false)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, new Uri("/panel/yukleme/" + id, UriKind.Relative)) { Content = content };
        if (noToken)
        {
            request.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        }

        return await Client.SendAsync(request, Ct);
    }

    public Task<HttpResponseMessage> StatusAsync(string id) => Client.GetAsync(new Uri("/panel/yukleme/" + id, UriKind.Relative), Ct);

    // --- Database ---

    public async Task<long> CountAsync(string sql)
    {
        await using AppDbContext db = NewContext();
        await using System.Data.Common.DbCommand command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        await db.Database.OpenConnectionAsync(Ct);
        return Convert.ToInt64(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>A digital product with one variant, a completed file of the given bytes attached to the variant (in the store too), and an order item that bought it.</summary>
    public async Task<(Guid ItemId, Guid VariantId, Guid FileId)> SeedDownloadAsync(byte[] content, string fileName = "kitap.pdf", ProductType itemType = ProductType.Digital)
    {
        DateTimeOffset now = Time.GetUtcNow();
        string n = Guid.NewGuid().ToString("N")[..8];
        await using AppDbContext db = NewContext();
        Product product = Product.Create("Kitap " + n, "kitap-" + n, ProductType.Digital, 20m, now);
        Variant variant = Variant.Create(product.Id, "SKU-" + n, 10m, now);
        db.Products.Add(product);
        db.Variants.Add(variant);
        await db.SaveChangesAsync(Ct);

        DigitalFile file = await AttachFileAsync(db, variant.Id, content, fileName);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(Dsn, null);
        OrderItem item = OrderItem.Create(orderId, product.Id, variant.Id, itemType, "Kitap", 1, 10m, 20m, 1.67m, now);
        if (itemType == ProductType.Digital)
        {
            item.DigitalFileId = file.Id;
            item.DownloadAllowance = 3;
        }

        db.OrderItems.Add(item);
        await db.SaveChangesAsync(Ct);
        return (item.Id, variant.Id, file.Id);
    }

    /// <summary>An order item whose variant link is empty and whose last known file is a started upload that never completed (an unattached, incomplete row).</summary>
    public async Task<Guid> SeedItemOfAnIncompleteFileAsync()
    {
        DateTimeOffset now = Time.GetUtcNow();
        await using AppDbContext db = NewContext();
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "yarim.pdf", 50, now);
        db.DigitalFiles.Add(file);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(Dsn, null);
        OrderItem item = OrderItem.Create(orderId, null, null, ProductType.Digital, "Kitap", 1, 10m, 20m, 1.67m, now);
        item.DigitalFileId = file.Id;
        item.DownloadAllowance = 3;
        db.OrderItems.Add(item);
        await db.SaveChangesAsync(Ct);
        return item.Id;
    }

    /// <summary>Detaches the variant's current file (if any) and attaches a new completed one with the given bytes; the file is written to the store.</summary>
    public async Task<DigitalFile> ReplaceFileAsync(Guid variantId, byte[] content, string fileName = "yeni.pdf")
    {
        await using AppDbContext db = NewContext();
        DateTimeOffset now = Time.GetUtcNow();
        foreach (DigitalFile current in await db.DigitalFiles.Where(f => f.VariantId == variantId && f.DetachedAt == null).ToListAsync(Ct))
        {
            current.DetachedAt = now;
        }

        await db.SaveChangesAsync(Ct);
        return await AttachFileAsync(db, variantId, content, fileName);
    }

    private async Task<DigitalFile> AttachFileAsync(AppDbContext db, Guid variantId, byte[] content, string fileName)
    {
        DateTimeOffset now = Time.GetUtcNow();
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), fileName, content.Length, now);
        file.VariantId = variantId;
        file.AttachedAt = now;
        file.UploadCompletedAt = now;
        using MemoryStream stream = new(content);
        await Files.WriteAsync(FileArea.DigitalFile, file.StorageKey, stream, Ct);
        db.DigitalFiles.Add(file);
        await db.SaveChangesAsync(Ct);
        return file;
    }

    /// <summary>Says to the real server that the request came over HTTPS through the trusted proxy (the loopback), as the site's cookies and its forgery token need.</summary>
    private sealed class ForwardedHttpsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
            return base.SendAsync(request, cancellationToken);
        }
    }

    /// <summary>The disk the application sees: 100 GB in all and 90 GB free until a test changes it.</summary>
    internal sealed class FakeDisk : IDiskUsageReader
    {
        public long Total { get; set; } = 100_000_000_000;

        public long Free { get; set; } = 90_000_000_000;

        public double UsedFraction() => 1.0 - ((double)Free / Total);

        public DiskCapacity Capacity() => new(Total, Free);
    }
}
