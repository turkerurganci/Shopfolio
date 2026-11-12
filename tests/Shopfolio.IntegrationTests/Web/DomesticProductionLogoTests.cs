using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The domestic production logo on the opened application (T501 KK1-KK3, KK5; 02 3.8.5; 08 4.3; 09 11.1 item 7): the file is served from the assembly under a hashed address in the static area,
/// and the one origin component draws it beside the price block's origin line - for a physical product made in Turkey only, with the country's name written in every case.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class DomesticProductionLogoTests
{
    private const string Logo = "Yerli üretim logosu";

    private readonly CollectionDatabaseFixture _database;

    public DomesticProductionLogoTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(OriginLabelSampleModel).Assembly)));

    private static async Task<string> PriceBlockAsync(HttpClient client, ProductType type, string? code, string? name)
    {
        string query = $"type={type}&code={Uri.EscapeDataString(code ?? string.Empty)}&name={Uri.EscapeDataString(name ?? string.Empty)}";
        using HttpResponseMessage response = await client.GetAsync($"/sss/ornek-mensei/ornek?{query}", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task TheLogoFile_IsServedFromTheAssemblyUnderItsHashedAddress_WithTheHeadersOfAStaticFile_T501_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(DomesticProductionLogo.Address, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/jpeg", response.Content.Headers.ContentType?.MediaType);
        byte[] body = await response.Content.ReadAsByteArrayAsync(Ct);
        Assert.Equal(DomesticProductionLogo.Content.ToArray(), body);
        Assert.Equal("public, max-age=31536000, immutable", response.Headers.CacheControl?.ToString());
        Assert.StartsWith("/statik/", DomesticProductionLogo.Address, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnAddressWithAnotherHash_IsNotTheLogo_AndNoValidatorHeaderIsWritten_T501_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage current = await client.GetAsync(DomesticProductionLogo.Address, Ct);
        using HttpResponseMessage other = await client.GetAsync("/statik/yerli-uretim-logosu.0000000000.jpg", Ct);

        Assert.Null(current.Headers.ETag);
        Assert.Equal(HttpStatusCode.NotFound, other.StatusCode);
    }

    [Fact]
    public async Task APhysicalProductMadeInTurkey_ShowsTheLogoBesideTheOriginLine_T501_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await PriceBlockAsync(client, ProductType.Physical, "TR", "Türkiye");

        Assert.Contains("Üretim yeri: Türkiye", html, StringComparison.Ordinal);
        Assert.Contains($"<img class=\"origin-label__logo\" src=\"{DomesticProductionLogo.Address}\" alt=\"{Logo}\"", html, StringComparison.Ordinal);
        Assert.Equal(1, html.Split("origin-label__logo", StringSplitOptions.None).Length - 1);
    }

    [Theory]
    [InlineData(ProductType.Digital)]
    [InlineData(ProductType.Service)]
    public async Task ADigitalProductAndAService_NeverShowTheLogo_T501_KK2(ProductType type)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await PriceBlockAsync(client, type, "TR", "Türkiye");

        Assert.DoesNotContain("origin-label__logo", html, StringComparison.Ordinal);
        Assert.DoesNotContain(Logo, html, StringComparison.Ordinal);
        Assert.Equal(type == ProductType.Digital, html.Contains("Üretim yeri: Türkiye", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AnotherCountryAndAnEmptyPlaceOfOrigin_ShowNoLogo_AndTheCountrysNameIsStillWritten_T501_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string germany = await PriceBlockAsync(client, ProductType.Physical, "DE", "Almanya");
        string empty = await PriceBlockAsync(client, ProductType.Physical, null, null);

        Assert.Contains("Üretim yeri: Almanya", germany, StringComparison.Ordinal);
        Assert.DoesNotContain("origin-label__logo", germany, StringComparison.Ordinal);
        Assert.DoesNotContain("origin-label", empty, StringComparison.Ordinal);
    }
}
