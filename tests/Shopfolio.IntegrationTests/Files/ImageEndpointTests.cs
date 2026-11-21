using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// The image address <c>/gorsel/{image id}</c> on the opened application and a real PostgreSQL (T604 D-1136, D-1711; K-1631 (1); 05 7.7.1): the address a page writes
/// (<see cref="FrameStyle.ImagePath"/>) answers the row's file with P-22's media type of the stored format, cached for good; an id that names no row, a row whose file is
/// not in the store and a digital file's key are a bare 404 - the digital file section is never reached.
/// </summary>
[Collection(FileEndpointsDefinition.Name)]
public sealed class ImageEndpointTests
{
    private static readonly byte[] Content = [.. Enumerable.Range(0, 64).Select(i => (byte)(i * 3))];

    private readonly CollectionDatabaseFixture _database;

    public ImageEndpointTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static string KeyOf(string seed) => Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("t604-" + seed)));

    private static async Task<Image> SeedAsync(FileEndpointsHost host, string seed, ImageFormat format, bool withFile = true)
    {
        Image image = Image.Create(KeyOf(seed), format, 1, 1, host.Time.GetUtcNow());
        if (withFile)
        {
            using MemoryStream stream = new(Content, writable: false);
            await host.Files.WriteAsync(FileArea.Image, image.StorageKey, stream, Ct);
        }

        await using AppDbContext db = host.NewContext();
        db.Images.Add(image);
        await db.SaveChangesAsync(Ct);
        return image;
    }

    [Fact]
    public async Task TheAddressAPageWrites_AnswersTheRowsFile_WithP22sTypeOfItsFormat_CachedForGood_D1136()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        Dictionary<ImageFormat, Image> images = [];
        foreach (ImageFormat format in Enum.GetValues<ImageFormat>())
        {
            images[format] = await SeedAsync(host, "type-" + format, format);
        }

        foreach ((ImageFormat format, Image image) in images)
        {
            using HttpResponseMessage response = await host.Client.GetAsync(new Uri(FrameStyle.ImagePath(image.Id), UriKind.Relative), Ct);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(Content, await response.Content.ReadAsByteArrayAsync(Ct));
            Assert.Equal(ProductConstants.ImageMediaTypes[(int)format], response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("public, max-age=31536000, immutable", string.Join(", ", response.Headers.GetValues("Cache-Control")));
            Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
            // The address is the validator (07 2.4.7): no tag and no date.
            Assert.Null(response.Headers.ETag);
            Assert.Null(response.Content.Headers.LastModified);
        }
    }

    [Fact]
    public async Task AnIdOfNoRow_ARowWithoutItsFile_AndADigitalFilesKey_AreABare404_D1711()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        Image orphan = await SeedAsync(host, "no-file", ImageFormat.Png, withFile: false);
        (_, _, Guid fileId) = await host.SeedDownloadAsync(Content);

        foreach (Guid id in new[] { Guid.NewGuid(), orphan.Id, fileId })
        {
            using HttpResponseMessage response = await host.Client.GetAsync(new Uri(FrameStyle.ImagePath(id), UriKind.Relative), Ct);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsByteArrayAsync(Ct));
        }
    }
}
