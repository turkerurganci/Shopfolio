using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The parts of the publish status transitions that need no database (T321; 03 1.8; 02 3.7.1, 5.1): what the visitor sees by status, the first publication's
/// marks, the result's codes and the one writer of the status. The transactional behaviour is the integration tests'.
/// </summary>
public sealed class PublishTransitionsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Later = Now.AddDays(3);

    private static readonly PublishStatus[] AllStatuses = Enum.GetValues<PublishStatus>();

    // ---- KK3, KK4, KK9: what the visitor sees ----

    [Fact]
    public void Listed_KeepsExactlyThePublishedProducts_OfEveryStatus_KK3_KK4()
    {
        List<Product> products = [.. AllStatuses.Select(status => ProductOf(status))];

        List<Product> listed = [.. products.AsQueryable().Listed()];

        Assert.Equal(products.Where(p => p.PublishStatus == PublishStatus.Published), listed);
        Assert.DoesNotContain(listed, p => p.PublishStatus != PublishStatus.Published);
        Assert.Single(listed);
    }

    [Fact]
    public void InOptionList_KeepsAVariantExactlyWhenItAndItsProductArePublished_InEveryPairOfStatuses_KK3_KK4()
    {
        List<Product> products = [.. AllStatuses.Select(status => ProductOf(status))];
        List<Variant> variants = [];
        foreach (Product product in products)
        {
            variants.AddRange(AllStatuses.Select(status => VariantOf(product, status)));
        }

        List<Variant> listed = [.. variants.AsQueryable().InOptionList(products.AsQueryable())];

        Dictionary<Guid, PublishStatus> productStatus = products.ToDictionary(p => p.Id, p => p.PublishStatus);
        Assert.Equal(
            variants.Where(v => v.PublishStatus == PublishStatus.Published && productStatus[v.ProductId] == PublishStatus.Published),
            listed);
        Assert.Equal(AllStatuses.Length * AllStatuses.Length, variants.Count);
        Assert.Single(listed);
    }

    [Fact]
    public void Visibility_DoesNotLookAtTheStock_ASoldOutPublishedRecordStaysListed_KK9()
    {
        Product product = ProductOf(PublishStatus.Published);
        Variant soldOut = VariantOf(product, PublishStatus.Published);
        soldOut.StockQuantity = 0;

        Assert.Single(new[] { product }.AsQueryable().Listed());
        Assert.Single(new[] { soldOut }.AsQueryable().InOptionList(new[] { product }.AsQueryable()));
    }

    // ---- first publication ----

    [Fact]
    public void ApplyPublishStatus_FirstPublication_FixesTheAddress_AndLaterMovesKeepIt_EvenInTheArchive()
    {
        Product product = ProductOf(PublishStatus.Draft);

        product.ApplyPublishStatus(PublishStatus.Published, Now);
        product.ApplyPublishStatus(PublishStatus.Archived, Later);
        product.ApplyPublishStatus(PublishStatus.Published, Later.AddDays(1));

        Assert.Equal(Now, product.SlugFixedAt);
        Assert.Equal(PublishStatus.Published, product.PublishStatus);
    }

    [Fact]
    public void ApplyPublishStatus_ADraftOrArchivedMove_DoesNotFixTheAddress()
    {
        foreach (PublishStatus status in AllStatuses.Where(s => s != PublishStatus.Published))
        {
            Product product = ProductOf(PublishStatus.Published);
            product.ApplyPublishStatus(status, Now);

            Assert.Equal(status, product.PublishStatus);
            Assert.Null(product.SlugFixedAt);
        }
    }

    [Fact]
    public void MarkFirstOnSale_WritesTheFirstMomentOnly()
    {
        Variant variant = VariantOf(ProductOf(PublishStatus.Published), PublishStatus.Published);

        variant.MarkFirstOnSale(Now);
        variant.MarkFirstOnSale(Later);

        Assert.Equal(Now, variant.FirstOnSaleAt);
    }

    // ---- the result ----

    [Fact]
    public void Result_ChangedAndUnchanged_AreNotRefusals_AndCarryNoCode()
    {
        Assert.All(new[] { PublishChangeResult.Changed, PublishChangeResult.Unchanged }, r =>
        {
            Assert.False(r.IsRejected);
            Assert.Null(r.Code);
            Assert.Empty(r.UnmetConditions);
        });
        Assert.True(PublishChangeResult.Changed.IsChanged);
        Assert.False(PublishChangeResult.Unchanged.IsChanged);
    }

    [Fact]
    public void Result_TheRefusals_CarryTheCodesOfTheRegistry_KK5_KK6()
    {
        Assert.Equal(ErrorCode.LastPublishedVariant, PublishChangeResult.LastPublishedVariant.Code);
        Assert.Equal(ErrorCode.RecordVersionConflict, PublishChangeResult.VersionConflict.Code);
        Assert.Equal(ErrorCode.NotFound, PublishChangeResult.NotFound.Code);
        PublishChangeResult gate = PublishChangeResult.GateIncomplete(["name", "variant"]);
        Assert.Equal(ErrorCode.PublishGateIncomplete, gate.Code);
        Assert.Equal(["name", "variant"], gate.UnmetConditions);
        Assert.All(new[] { gate, PublishChangeResult.LastPublishedVariant, PublishChangeResult.VersionConflict, PublishChangeResult.NotFound }, r =>
        {
            Assert.True(r.IsRejected);
            Assert.False(r.IsChanged);
        });
    }

    [Fact]
    public void TheVerdict_IsOpenExactlyWhenNoConditionIsUnmet_KK3()
    {
        Assert.True(PublishGateVerdict.Open.IsOpen);
        Assert.False(new PublishGateVerdict(["file"]).IsOpen);
    }

    // ---- the one writer of the status ----

    [Fact]
    public void ApplicationSource_WritesThePublishStatusOfAProductAndAVariantOnlyInTheTransitionUnit_KK2()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (relative.Contains("/obj/", StringComparison.Ordinal) || relative.Contains("/bin/", StringComparison.Ordinal))
            {
                continue;
            }

            scanned.Add(relative);
            findings.AddRange(PublishStatusWriterScan.Scan(relative, File.ReadAllText(file)).Select(line => $"{relative}:{line}"));
        }

        Assert.Contains(PublishStatusWriterScan.UnitFile, scanned);
        Assert.Empty(findings);
    }

    [Fact]
    public void Scan_OnACallOutsideTheUnit_ReportsEachWriter_AndOnTheUnitsOwnFileNothing()
    {
        const string text = "product.ApplyPublishStatus(status, now);\nvariant.ApplyPublishStatus(status);\nvariant.MarkFirstOnSale(now);\n// ApplyPublishStatus(\n"
            + "product.PublishStatus = PublishStatus.Published;\nif (product.PublishStatus == PublishStatus.Draft)";

        Assert.Equal([1, 2, 3, 5], PublishStatusWriterScan.Scan("src/Shopfolio.Web/Modules/Catalog/Other.cs", text));
        Assert.Empty(PublishStatusWriterScan.Scan(PublishStatusWriterScan.UnitFile, text));
    }

    [Fact]
    public void Scan_OnAFileOfTheUnitsNameElsewhere_IsNotTheAllowedPlace()
    {
        Assert.Equal([1], PublishStatusWriterScan.Scan("src/Shopfolio.Web/Modules/Ordering/ProductTransitions.cs", "variant.ApplyPublishStatus(status);"));
    }

    [Fact]
    public void Scan_OnTheDeclarations_ReportsNothing()
    {
        const string text = "internal void ApplyPublishStatus(PublishStatus status) => PublishStatus = status;\ninternal void MarkFirstOnSale(DateTimeOffset now) => FirstOnSaleAt ??= now;";

        Assert.Empty(PublishStatusWriterScan.Scan("src/Shopfolio.Web/Modules/Catalog/Variant.cs", text));
    }

    private static Product ProductOf(PublishStatus status)
    {
        Product product = Product.Create("Masa", $"masa-{Guid.NewGuid():N}"[..12], ProductType.Physical, 20m, Now);
        product.PublishStatus = status;
        return product;
    }

    private static Variant VariantOf(Product product, PublishStatus status)
    {
        Variant variant = Variant.Create(product.Id, $"SKU-{Guid.NewGuid():N}", 100m, Now);
        variant.PublishStatus = status;
        return variant;
    }
}

/// <summary>
/// The source scan of the status writer (T321 KK2; 09 9.1 item 4): the calls that write a product's or a variant's publish status and the first sale moment are in
/// the transition unit's file and nowhere else. The allowed place is the unit's full path, not a file name: a file of the same name elsewhere is a finding.
/// </summary>
internal static class PublishStatusWriterScan
{
    public const string UnitFile = "src/Shopfolio.Web/Modules/Catalog/ProductTransitions.cs";

    private static readonly string[] Writers = ["ApplyPublishStatus(", "MarkFirstOnSale("];

    private static readonly string[] Declarations = ["void ApplyPublishStatus(", "void MarkFirstOnSale("];

    /// <summary>A direct write of a product's or a variant's status (D-301): an assignment, not a comparison. The setter stays public for the
    /// tests' seeds only; in the application it is written by the two Apply methods of its own files.</summary>
    private static readonly Regex StatusAssignment = new(@"\bPublishStatus\s*=(?!=)", RegexOptions.CultureInvariant);

    private static readonly string[] AssignmentOwners =
        ["src/Shopfolio.Web/Modules/Catalog/Product.cs", "src/Shopfolio.Web/Modules/Catalog/Variant.cs"];

    /// <summary>The corporate content record has a status of the same name and its own writer (ContentPublishing).</summary>
    private const string ContentModule = "src/Shopfolio.Web/Modules/CorporateContent/";

    /// <summary>The 1-based lines of a call outside the unit's file; a comment line and a declaration are not calls.</summary>
    public static IReadOnlyList<int> Scan(string relativePath, string text)
    {
        if (string.Equals(relativePath, UnitFile, StringComparison.Ordinal))
        {
            return [];
        }

        List<int> lines = [];
        string[] all = text.Split('\n');
        for (int i = 0; i < all.Length; i++)
        {
            string line = all[i].Trim();
            if (line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith('*') || line.StartsWith("///", StringComparison.Ordinal))
            {
                continue;
            }

            bool call = Writers.Any(w => line.Contains(w, StringComparison.Ordinal)) && !Declarations.Any(d => line.Contains(d, StringComparison.Ordinal));
            bool assignment = !AssignmentOwners.Contains(relativePath, StringComparer.Ordinal)
                && !relativePath.StartsWith(ContentModule, StringComparison.Ordinal)
                && StatusAssignment.IsMatch(line);
            if (call || assignment)
            {
                lines.Add(i + 1);
            }
        }

        return lines;
    }
}
