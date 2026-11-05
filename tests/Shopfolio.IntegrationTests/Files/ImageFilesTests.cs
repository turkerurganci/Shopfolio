using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.UnitTests.Fakes;
using Shopfolio.UnitTests.Imaging;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// The image files of the Catalog module on a real PostgreSQL (T223 criteria 5-9; 09 15.6; 05 7.7.1, 7.7.2; 06 4.69, 9.3.4): the upload's row and
/// derivative job written in one transaction under the content's fingerprint, the derivative job run twice leaving the same files and one moment,
/// the disposal of images no link names and of files no row names - the record before the files - and the delete statement that reads every link
/// to the image table from the model. Every row and file is aged relative to the harness's clock.
/// </summary>
[Collection(FilesDefinition.Name)]
public sealed class ImageFilesTests : IAsyncLifetime
{
    private readonly CollectionDatabaseFixture _collection;
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "shopfolio-t223-" + Guid.NewGuid().ToString("N"));
    private FilesHarness _disk = null!;

    public ImageFilesTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using (NpgsqlConnection connection = new(_collection.Dsn))
        {
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand truncate = new(
                "TRUNCATE jobs, disposal_records, product_images, variants, products, brandings, images CASCADE", connection);
            await truncate.ExecuteNonQueryAsync(Ct);
        }

        _disk = new FilesHarness(_collection.Dsn, time => new DiskFileStore(new FileStoreDirectories(Path.Combine(_directory, "images"), Path.Combine(_directory, "digital")), time));
    }

    public async ValueTask DisposeAsync()
    {
        await _disk.DisposeAsync();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    // ---- KK5: the upload ----

    [Fact]
    public async Task AnUpload_WritesTheSourceCopyUnderItsFingerprint_AndTheRowAndTheJobInOneTransaction()
    {
        ImageUploadResult result = await _disk.UploadAsync(ImageFixtures.Jpeg(1200, 800));

        Guid id = result.ImageId ?? throw new InvalidOperationException("The upload did not succeed.");
        Image image = await _disk.ReadAsync(db => db.Images.AsNoTracking().SingleAsync(i => i.Id == id, Ct));
        await using Stream file = await _disk.Files.OpenReadAsync(FileArea.Image, image.StorageKey, Ct);
        using MemoryStream copy = new();
        await file.CopyToAsync(copy, Ct);
        Job job = await _disk.ReadAsync(db => db.Jobs.AsNoTracking().SingleAsync(Ct));

        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(copy.ToArray())), image.StorageKey);
        Assert.Equal((1200, 800, ImageFormat.Jpeg, (DateTimeOffset?)null), (image.Width, image.Height, image.ImageFormat, image.DerivativesReadyAt));
        Assert.Equal((JobType.ImageDerivative, id, JobState.Pending), (job.JobType, job.RelatedRecordId, job.JobState));
    }

    [Fact]
    public async Task TheSameContentTwice_PointsToOneFile_AndGetsTwoRowsAndTwoJobs()
    {
        byte[] content = ImageFixtures.Png(640, 480);

        Guid first = (await _disk.UploadAsync(content)).ImageId!.Value;
        Guid second = (await _disk.UploadAsync(content)).ImageId!.Value;

        List<Image> images = await _disk.ReadAsync(db => db.Images.AsNoTracking().ToListAsync(Ct));
        List<string> keys = await ListKeysAsync(_disk.Files, TimeSpan.Zero);
        long jobs = await _disk.ReadAsync(db => db.Jobs.LongCountAsync(Ct));

        Assert.NotEqual(first, second);
        Assert.Equal(2, images.Count);
        Assert.Single(images.Select(i => i.StorageKey).Distinct());
        Assert.Equal([images[0].StorageKey], keys);
        Assert.Equal(2L, jobs);
    }

    [Fact]
    public async Task ARejectedUpload_WritesNoFileNoRowAndNoJob()
    {
        ImageUploadResult result = await _disk.UploadAsync(ImageFixtures.Svg());

        Assert.Null(result.ImageId);
        Assert.Empty(await ListKeysAsync(_disk.Files, TimeSpan.Zero));
        Assert.Equal(0L, await _disk.ReadAsync(db => db.Images.LongCountAsync(Ct)));
        Assert.Equal(0L, await _disk.ReadAsync(db => db.Jobs.LongCountAsync(Ct)));
    }

    [Fact]
    public async Task AnUploadOfExactlyTheCeiling_IsAccepted_AndOneByteMore_IsRefusedBeforeAnythingIsRead()
    {
        byte[] image = ImageFixtures.Png(300, 200);
        byte[] atCeiling = new byte[ProductConstants.ImageMaxBytes];
        image.CopyTo(atCeiling, 0);
        byte[] overCeiling = new byte[ProductConstants.ImageMaxBytes + 1];
        image.CopyTo(overCeiling, 0);

        ImageUploadResult over = await _disk.UploadAsync(overCeiling);
        ImageUploadResult at = await _disk.UploadAsync(atCeiling);

        Assert.Same(ImageUploadResult.TooLarge, over);
        Assert.Equal("TooLarge", over.Reason);
        Assert.NotNull(at.ImageId);
        Assert.Equal(1L, await _disk.ReadAsync(db => db.Images.LongCountAsync(Ct)));
        Assert.Single(await ListKeysAsync(_disk.Files, TimeSpan.Zero));
    }

    // ---- 09 9.1 item 3: the module joins the caller's transaction ----

    [Fact]
    public async Task TheUpload_JoinsTheCallersTransaction_SoTheCallersOwnWorkAndTheUploadCommitOrRollBackTogether()
    {
        Guid kept = Guid.Empty;
        ImageUploadResult committed = await _disk.InScopeAsync(services => services.GetRequiredService<ITransactionRunner>().RunAsync(
            async cancel =>
            {
                ImageUploadResult first = await services.GetRequiredService<ImageUploads>().UploadAsync(ImageFixtures.Png(120, 80), cancel);
                ImageUploadResult second = await services.GetRequiredService<ImageUploads>().UploadAsync(ImageFixtures.Jpeg(120, 80), cancel);
                kept = second.ImageId!.Value;
                Assert.NotNull(services.GetRequiredService<AppDbContext>().Database.CurrentTransaction);
                return first;
            },
            Ct));
        Assert.NotNull(committed.ImageId);
        Assert.Equal(2L, await _disk.ReadAsync(db => db.Images.LongCountAsync(Ct)));
        Assert.Equal(2L, await _disk.ReadAsync(db => db.Jobs.LongCountAsync(Ct)));

        // A refusal by the caller after the upload rolls the upload's row and job back with it; only the file is left (the sweep removes it).
        ImageUploadResult refused = await _disk.InScopeAsync(services => services.GetRequiredService<ITransactionRunner>().RunAsync(
            async cancel =>
            {
                await services.GetRequiredService<ImageUploads>().UploadAsync(ImageFixtures.Png(90, 60), cancel);
                return ImageUploadResult.Undecodable;
            },
            Ct));
        Assert.True(refused.IsRejected);
        Assert.Equal(2L, await _disk.ReadAsync(db => db.Images.LongCountAsync(Ct)));
        Assert.Equal(2L, await _disk.ReadAsync(db => db.Jobs.LongCountAsync(Ct)));
        Assert.NotEqual(Guid.Empty, kept);
    }

    [Fact]
    public async Task TheUpload_NeverOpensATransactionOfItsOwn_AndWithoutOneTheJobRowRefusesToBeWritten()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _disk.UploadWithoutTransactionAsync(ImageFixtures.Png(120, 80)));

        Assert.Equal(0L, await _disk.ReadAsync(db => db.Images.LongCountAsync(Ct)));
        Assert.Equal(0L, await _disk.ReadAsync(db => db.Jobs.LongCountAsync(Ct)));
    }

    // ---- KK6: the derivative job ----

    [Fact]
    public async Task TheDerivativeJob_WritesTheNarrowerWidths_AndASecondRunChangesNeitherFilesNorTheMoment()
    {
        Guid id = (await _disk.UploadAsync(ImageFixtures.Png(2000, 1000))).ImageId!.Value;
        Image image = await _disk.ReadAsync(db => db.Images.AsNoTracking().SingleAsync(i => i.Id == id, Ct));

        await _disk.RunDerivativeJobAsync(id);
        Dictionary<string, byte[]> afterFirst = await ReadAllAsync(_disk.Files);
        DateTimeOffset? firstMoment = await _disk.ReadAsync(db => db.Images.Where(i => i.Id == id).Select(i => i.DerivativesReadyAt).SingleAsync(Ct));
        _disk.Time.Advance(TimeSpan.FromMinutes(5));
        await _disk.RunDerivativeJobAsync(id);
        Dictionary<string, byte[]> afterSecond = await ReadAllAsync(_disk.Files);
        DateTimeOffset? secondMoment = await _disk.ReadAsync(db => db.Images.Where(i => i.Id == id).Select(i => i.DerivativesReadyAt).SingleAsync(Ct));

        string[] expected = [image.StorageKey, .. ImageServing.DerivativeWidthsFor(2000).Select(w => FileKeys.DerivativeKey(image.StorageKey, w))];
        Assert.Equal(4, expected.Length);
        Assert.Equal(expected.Order(StringComparer.Ordinal), afterFirst.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(afterFirst.Keys.Order(StringComparer.Ordinal), afterSecond.Keys.Order(StringComparer.Ordinal));
        foreach (string key in afterFirst.Keys)
        {
            Assert.Equal(afterFirst[key], afterSecond[key]);
        }

        foreach (int width in ImageServing.DerivativeWidthsFor(2000))
        {
            Assert.Equal(width, ImageInspection.Read(afterFirst[FileKeys.DerivativeKey(image.StorageKey, width)]).Width);
        }

        Assert.Equal(_disk.Now.AddMinutes(-5), firstMoment);
        Assert.Equal(firstMoment, secondMoment);
    }

    [Fact]
    public async Task TheDerivativeJob_MakesNoDerivativeAsWideAsTheSource_ButStillWritesTheMoment()
    {
        Guid id = (await _disk.UploadAsync(ImageFixtures.Jpeg(400, 300))).ImageId!.Value;

        JobOutcome outcome = await _disk.RunDerivativeJobAsync(id);

        Assert.Equal(JobOutcome.Completed, outcome);
        Assert.Single(await ListKeysAsync(_disk.Files, TimeSpan.Zero));
        Assert.Equal(_disk.Now, await _disk.ReadAsync(db => db.Images.Where(i => i.Id == id).Select(i => i.DerivativesReadyAt).SingleAsync(Ct)));
    }

    [Fact]
    public async Task TheDerivativeJob_ForADeletedImage_CompletesAndWritesNothing()
    {
        JobOutcome outcome = await _disk.RunDerivativeJobAsync(Guid.NewGuid());

        Assert.Equal(JobOutcome.Completed, outcome);
        Assert.Empty(await ListKeysAsync(_disk.Files, TimeSpan.Zero));
    }

    // ---- KK9: the disposal of the images ----

    [Fact]
    public async Task TheImageDisposal_DeletesTheUnlinkedOldImages_AndKeepsTheLinkedOnesAndTheYoungOnes()
    {
        DateTimeOffset old = _disk.Now.AddDays(-2);
        Image unlinkedOld = Image.Create(Key('1'), ImageFormat.Jpeg, 10, 10, old);
        Image unlinkedYoung = Image.Create(Key('2'), ImageFormat.Jpeg, 10, 10, _disk.Now.AddHours(-23));
        Image gallery = Image.Create(Key('3'), ImageFormat.Jpeg, 10, 10, old);
        Image variantImage = Image.Create(Key('4'), ImageFormat.Jpeg, 10, 10, old);
        Image logo = Image.Create(Key('5'), ImageFormat.Jpeg, 10, 10, old);
        Product product = Product.Create("Ürün", "urun", ProductType.Physical, 20m, old);
        Variant variant = Variant.Create(product.Id, "SKU-1", 5m, old);
        variant.ImageId = variantImage.Id;
        Branding branding = Branding.Create(old);
        branding.LogoId = logo.Id;
        await _disk.SeedAsync(db =>
        {
            db.Images.AddRange(unlinkedOld, unlinkedYoung, gallery, variantImage, logo);
            db.Products.Add(product);
            db.Variants.Add(variant);
            db.Brandings.Add(branding);
        });
        await _disk.SeedAsync(db => db.ProductImages.Add(ProductImage.Create(product.Id, gallery.Id, 1, true, old)));

        await _disk.RunDisposalAsync<ImageDisposalPart>();

        Guid[] left = await _disk.ReadAsync(db => db.Images.Select(i => i.Id).ToArrayAsync(Ct));
        Assert.Equal(new[] { unlinkedYoung.Id, gallery.Id, variantImage.Id, logo.Id }.Order(), left.Order());
        DisposalRecord record = await _disk.ReadAsync(db => db.DisposalRecords.AsNoTracking().SingleAsync(Ct));
        Assert.Equal((DisposalRecordType.Image, 1), (record.DisposalRecordType, record.Count));
    }

    [Fact]
    public async Task AnImageDeletedByTheDisposal_IsNotDeletedWhileALinkNamesIt_EvenOnASecondRun()
    {
        DateTimeOffset old = _disk.Now.AddDays(-3);
        Image linked = Image.Create(Key('a'), ImageFormat.Png, 10, 10, old);
        Branding branding = Branding.Create(old);
        branding.FaviconId = linked.Id;
        await _disk.SeedAsync(db =>
        {
            db.Images.Add(linked);
            db.Brandings.Add(branding);
        });

        await _disk.RunDisposalAsync<ImageDisposalPart>();
        await _disk.RunDisposalAsync<ImageDisposalPart>();

        Assert.Equal(1L, await _disk.ReadAsync(db => db.Images.LongCountAsync(Ct)));
        Assert.Equal(0L, await _disk.ReadAsync(db => db.DisposalRecords.LongCountAsync(Ct)));
    }

    [Fact]
    public async Task TheDeleteStatement_NamesEveryTableThatHoldsAForeignKeyToTheImageTable()
    {
        List<string> tables = [];
        await using (NpgsqlConnection connection = new(_collection.Dsn))
        {
            await connection.OpenAsync(Ct);
            await using NpgsqlCommand query = new(
                "SELECT c.conrelid::regclass::text FROM pg_constraint c WHERE c.contype = 'f' AND c.confrelid = 'images'::regclass ORDER BY 1",
                connection);
            await using NpgsqlDataReader reader = await query.ExecuteReaderAsync(Ct);
            while (await reader.ReadAsync(Ct))
            {
                tables.Add(reader.GetString(0).Trim('"'));
            }
        }

        string statement = await _disk.ReadAsync(db => Task.FromResult(ImageDisposalPart.DeleteStatement(db)));

        Assert.True(tables.Distinct().Count() > 4, "The database has far more tables linking to images than the four seeded in these tests.");
        Assert.Contains("product_images", tables);
        Assert.Contains("variants", tables);
        Assert.Contains("brandings", tables);
        foreach (string table in tables.Distinct())
        {
            Assert.Contains($"FROM \"{table}\" l", statement, StringComparison.Ordinal);
        }

        Assert.Equal(tables.Count, statement.Split("NOT EXISTS", StringSplitOptions.None).Length - 1);
    }

    // ---- KK9: the files no row names ----

    [Fact]
    public async Task TheOrphanFileSweep_WritesTheRecordBeforeItDeletesAFile_AndKeepsTheFilesARowNames()
    {
        List<long> recordsAtEachDeletion = [];
        FilesHarness fake = null!;
        InMemoryFileStore? memory = null;
        fake = new FilesHarness(_collection.Dsn, time =>
        {
            memory = new InMemoryFileStore(time);
            return new RecordCountingStore(memory, async () => await fake!.ReadAsync(db => db.DisposalRecords.LongCountAsync(Ct)), recordsAtEachDeletion);
        });
        await using (fake)
        {
            DateTimeOffset old = fake.Now.AddDays(-2);
            string named = Key('b');
            string orphan = Key('c');
            string youngOrphan = Key('d');
            string derivativeOfNamed = FileKeys.DerivativeKey(named, 480);
            string derivativeOfOrphan = FileKeys.DerivativeKey(orphan, 480);
            memory!.Seed(FileArea.Image, named, [1], old);
            memory.Seed(FileArea.Image, derivativeOfNamed, [2], old);
            memory.Seed(FileArea.Image, orphan, [3], old);
            memory.Seed(FileArea.Image, derivativeOfOrphan, [4], old);
            memory.Seed(FileArea.Image, youngOrphan, [5], fake.Now.AddHours(-23));
            await fake.SeedAsync(db => db.Images.Add(Image.Create(named, ImageFormat.Jpeg, 2000, 1000, old)));

            await fake.RunDisposalAsync<OrphanImageFilePart>();

            Assert.Equal(new[] { named, derivativeOfNamed, youngOrphan }.Order(StringComparer.Ordinal), memory.KeysOf(FileArea.Image));
            DisposalRecord record = await fake.ReadAsync(db => db.DisposalRecords.AsNoTracking().SingleAsync(Ct));
            Assert.Equal((DisposalRecordType.OrphanFile, 2), (record.DisposalRecordType, record.Count));
            Assert.Equal([1L, 1L], recordsAtEachDeletion);
        }
    }

    [Fact]
    public async Task TheOrphanFileSweep_KeepsADerivativeWhoseSourceRowIsThere_AndRemovesItOnceTheRowIsGone()
    {
        DateTimeOffset old = _disk.Now.AddDays(-2);
        Image image = Image.Create(Key('e'), ImageFormat.Png, 2000, 1000, old);
        await _disk.SeedAsync(db => db.Images.Add(image));
        await WriteAsync(_disk.Files, image.StorageKey, [1]);
        await WriteAsync(_disk.Files, FileKeys.DerivativeKey(image.StorageKey, 960), [2]);
        AgeFiles(old);

        await _disk.RunDisposalAsync<OrphanImageFilePart>();
        Assert.Equal(2, (await ListKeysAsync(_disk.Files, TimeSpan.Zero)).Count);

        await _disk.RunDisposalAsync<ImageDisposalPart>();
        await _disk.RunDisposalAsync<OrphanImageFilePart>();

        Assert.Empty(await ListKeysAsync(_disk.Files, TimeSpan.Zero));
    }

    private static string Key(char digit) => new(digit, FileKeys.ImageFingerprintLength);

    private static async Task WriteAsync(IFileStore files, string key, byte[] content)
    {
        using MemoryStream stream = new(content, writable: false);
        await files.WriteAsync(FileArea.Image, key, stream, Ct);
    }

    private void AgeFiles(DateTimeOffset writtenAt)
    {
        foreach (string path in Directory.EnumerateFiles(Path.Combine(_directory, "images")))
        {
            File.SetLastWriteTimeUtc(path, writtenAt.UtcDateTime);
        }
    }

    private static async Task<List<string>> ListKeysAsync(IFileStore files, TimeSpan minAge)
    {
        List<string> keys = [];
        await foreach (string key in files.ListAsync(FileArea.Image, minAge, Ct))
        {
            keys.Add(key);
        }

        return keys;
    }

    private static async Task<Dictionary<string, byte[]>> ReadAllAsync(IFileStore files)
    {
        Dictionary<string, byte[]> all = [];
        foreach (string key in await ListKeysAsync(files, TimeSpan.Zero))
        {
            await using Stream stream = await files.OpenReadAsync(FileArea.Image, key, Ct);
            using MemoryStream copy = new();
            await stream.CopyToAsync(copy, Ct);
            all[key] = copy.ToArray();
        }

        return all;
    }
}

/// <summary>A file store that tells how many disposal records existed at the moment each file was deleted: a record must come first.</summary>
internal sealed class RecordCountingStore : IFileStore
{
    private readonly IFileStore _inner;
    private readonly Func<Task<long>> _records;
    private readonly List<long> _seen;

    public RecordCountingStore(IFileStore inner, Func<Task<long>> records, List<long> seen)
    {
        _inner = inner;
        _records = records;
        _seen = seen;
    }

    public Task WriteAsync(FileArea area, string key, Stream content, CancellationToken ct) => _inner.WriteAsync(area, key, content, ct);

    public Task AppendAsync(FileArea area, string key, Stream content, CancellationToken ct) => _inner.AppendAsync(area, key, content, ct);

    public Task<Stream> OpenReadAsync(FileArea area, string key, CancellationToken ct) => _inner.OpenReadAsync(area, key, ct);

    public Task<bool> ExistsAsync(FileArea area, string key, CancellationToken ct) => _inner.ExistsAsync(area, key, ct);

    public Task<long> GetSizeAsync(FileArea area, string key, CancellationToken ct) => _inner.GetSizeAsync(area, key, ct);

    public async Task DeleteAsync(FileArea area, string key, CancellationToken ct)
    {
        _seen.Add(await _records());
        await _inner.DeleteAsync(area, key, ct);
    }

    public IAsyncEnumerable<string> ListAsync(FileArea area, TimeSpan minAge, CancellationToken ct) => _inner.ListAsync(area, minAge, ct);
}
