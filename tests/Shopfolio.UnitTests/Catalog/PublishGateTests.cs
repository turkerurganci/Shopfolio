using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Catalog;

/// <summary>
/// The parts of the publish gate, the archive page and the removals that need no database (T335; 02 3.7.1-3.7.8; 10 KD-28): the gate's conditions as pure functions, what each
/// address answer shows, the results' codes, the closed condition set that looks at nothing about what the product is, and the one place that deletes a product or a variant.
/// The transactional behaviour is the integration tests'.
/// </summary>
public sealed class PublishGateTests
{
    private const string Name = "Masa";

    // ---- KK3: the conditions of the gate ----

    [Fact]
    public void AProductWithEveryCondition_OpensTheGate_AndTheDescriptionAndTheImageAreNotAsked_KK3()
    {
        Assert.Empty(PublishGateRules.UnmetOfProduct(Name, true, ProductType.Physical, null, 1, 0));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankName_IsAnUnmetCondition_KK3(string? name)
    {
        Assert.Equal([PublishCondition.Name], PublishGateRules.UnmetOfProduct(name, true, ProductType.Physical, null, 1, 0));
    }

    [Fact]
    public void NoMainCategory_IsAnUnmetCondition_KK3()
    {
        Assert.Equal([PublishCondition.MainCategory], PublishGateRules.UnmetOfProduct(Name, false, ProductType.Physical, null, 1, 0));
    }

    [Fact]
    public void ATypeOutsideTheThree_IsAnUnmetCondition_KK3()
    {
        Assert.Equal([PublishCondition.ProductType], PublishGateRules.UnmetOfProduct(Name, true, (ProductType)99, null, 1, 0));
    }

    [Fact]
    public void NoPublishedVariant_IsAnUnmetCondition_ForEveryType_KK3()
    {
        foreach (ProductType type in Enum.GetValues<ProductType>())
        {
            Assert.Contains(PublishCondition.PublishedVariant, PublishGateRules.UnmetOfProduct(Name, true, type, 5, 0, 0));
        }
    }

    [Fact]
    public void EveryUnmetCondition_IsNamed_InTheOrderOfTheList_KK3()
    {
        IReadOnlyList<string> unmet = PublishGateRules.UnmetOfProduct(null, false, (ProductType)99, null, 0, 0);

        Assert.Equal([PublishCondition.Name, PublishCondition.MainCategory, PublishCondition.ProductType, PublishCondition.PublishedVariant], unmet);
        Assert.Equal(PublishCondition.All.Where(unmet.Contains), unmet);
    }

    // ---- KK4: the file of a digital variant, the fulfilment period of a service ----

    [Fact]
    public void ADigitalProduct_WithAPublishedVariantWithoutAFile_IsRefused_AndOtherTypesAreNotAskedForAFile_KK4()
    {
        Assert.Equal([PublishCondition.DigitalFile], PublishGateRules.UnmetOfProduct(Name, true, ProductType.Digital, null, 2, 1));
        Assert.Empty(PublishGateRules.UnmetOfProduct(Name, true, ProductType.Digital, null, 2, 0));
        Assert.Empty(PublishGateRules.UnmetOfProduct(Name, true, ProductType.Physical, null, 2, 1));
        Assert.Empty(PublishGateRules.UnmetOfProduct(Name, true, ProductType.Service, 3, 2, 1));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(30, true)]
    public void AServiceProduct_NeedsItsFulfilmentPeriod_ByTheOneRuleOfTheDefinition_KK4(int? period, bool isOpen)
    {
        IReadOnlyList<string> unmet = PublishGateRules.UnmetOfProduct(Name, true, ProductType.Service, period, 1, 0);

        Assert.Equal(isOpen, unmet.Count == 0);
        Assert.Equal(ProductDefinitionRules.IsFulfilmentPeriodEntered(period), isOpen);
        Assert.All(unmet, condition => Assert.Equal(PublishCondition.FulfilmentPeriod, condition));
    }

    [Fact]
    public void APhysicalProduct_IsNotAskedForAFulfilmentPeriod_KK4()
    {
        Assert.Empty(PublishGateRules.UnmetOfProduct(Name, true, ProductType.Physical, null, 1, 0));
        Assert.Empty(PublishGateRules.UnmetOfProduct(Name, true, ProductType.Digital, null, 1, 0));
    }

    [Fact]
    public void ADigitalVariant_NeedsItsFile_AndOtherVariantsDoNot_KK4()
    {
        Assert.Equal([PublishCondition.DigitalFile], PublishGateRules.UnmetOfVariant(ProductType.Digital, false));
        Assert.Empty(PublishGateRules.UnmetOfVariant(ProductType.Digital, true));
        Assert.Empty(PublishGateRules.UnmetOfVariant(ProductType.Physical, false));
        Assert.Empty(PublishGateRules.UnmetOfVariant(ProductType.Service, false));
    }

    // T525 criterion 8 (07 UC-115; 02 3.7.3): under a published product a variant whose price is zero does not go Published. The database's check keeps a zero price out of the table,
    // so the rule is pinned here, on its pure form.
    [Theory]
    [InlineData("0", false)]
    [InlineData("-0.01", false)]
    [InlineData("0.01", true)]
    [InlineData("100", true)]
    public void AVariantGoesPublished_UnderAPublishedProduct_OnlyWithAPriceAboveZero_T525_KK8(string price, bool expected)
    {
        Assert.Equal(expected, PublishGateRules.HasSalePrice(decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture)));
    }

    // ---- KK10: the gate does not look at what the product is ----

    [Fact]
    public void TheConditionSet_IsClosed_AndNamesNothingAboutWhatTheProductIs_KK10()
    {
        Assert.Equal(
            ["Name", "MainCategory", "ProductType", "PublishedVariant", "DigitalFile", "FulfilmentPeriod"],
            PublishCondition.All);
        string[] constants = [.. typeof(PublishCondition).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral).Select(f => (string)f.GetRawConstantValue()!)];
        Assert.Equal(PublishCondition.All.Order(), constants.Order());
    }

    [Fact]
    public void TheGateAnswer_DoesNotDependOnTheNameOrTheCategoryOrTheProductsNature_KK10()
    {
        // Whatever the product is called, the same facts give the same answer: nothing in the gate reads a word of the name.
        string[] names = ["Sigara", "Alkollü içki", "Silah", "Reçeteli ilaç", "Yetişkin ürünü", "Masa"];

        Assert.All(names, name => Assert.Empty(PublishGateRules.UnmetOfProduct(name, true, ProductType.Physical, null, 1, 0)));
    }

    [Fact]
    public void TheModelAndTheGate_CarryNoFieldOfAgeCheckPermissionBlockOrLegalDocument_KK10()
    {
        string[] forbidden = ["Age", "Birth", "Adult", "Permit", "Permission", "Licence", "License", "Restricted", "Prohibited", "Banned", "Blocked", "Block", "Forbidden", "Regulation", "Legislation", "Certificate", "Consent", "Approval"];
        List<string> names = [];
        foreach (Type type in new[] { typeof(Product), typeof(Variant), typeof(DigitalFile), typeof(Category), typeof(ProductCategory) })
        {
            names.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(p => $"{type.Name}.{p.Name}"));
        }

        names.AddRange(PublishCondition.All.Select(c => $"PublishCondition.{c}"));
        names.AddRange(Enum.GetNames<ProductType>().Select(n => $"ProductType.{n}"));
        names.AddRange(typeof(PublishGateRules).GetMethods(BindingFlags.Public | BindingFlags.Static).Select(m => $"PublishGateRules.{m.Name}"));
        names.AddRange(typeof(PublishGateRules).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(m => m.GetParameters()).Select(p => $"PublishGateRules.{p.Name}"));

        List<string> findings = [.. names.Where(name => WordsOf(name).Any(word => forbidden.Contains(word, StringComparer.OrdinalIgnoreCase)))];

        Assert.Empty(findings);
        Assert.Contains("Product.Name", names);
    }

    [Fact]
    public void TheForbiddenWordCheck_FindsAWordInsideACompoundName_AndLeavesLongerWordsAlone()
    {
        Assert.Equal(["Min", "Age", "Check"], WordsOf("X.MinAgeCheck").Skip(1));
        Assert.Equal(["Message", "Count"], WordsOf("X.MessageCount").Skip(1));
        Assert.DoesNotContain("Age", WordsOf("Product.Percentage"));
    }

    private static IEnumerable<string> WordsOf(string name) => Regex.Matches(name, "[A-Z][a-z0-9]*|[a-z0-9]+").Select(m => m.Value);

    // ---- KK6, KK7: what an address answers ----

    [Fact]
    public void ThePageKinds_ShowWhatTheyShow_KK6_KK7()
    {
        ProductPage published = PageOf(ProductPageKind.Published);
        ProductPage draft = PageOf(ProductPageKind.DraftPreview);
        ProductPage archived = PageOf(ProductPageKind.Archived);

        Assert.Equal((false, true, true, true, false), (published.ShowsDraftBand, published.ShowsPrice, published.OffersCart, published.IsIndexable, published.IsNoLongerSold));
        Assert.Equal((true, true, false, false, false), (draft.ShowsDraftBand, draft.ShowsPrice, draft.OffersCart, draft.IsIndexable, draft.IsNoLongerSold));
        Assert.Equal((false, false, false, false, true), (archived.ShowsDraftBand, archived.ShowsPrice, archived.OffersCart, archived.IsIndexable, archived.IsNoLongerSold));
        Assert.Equal(ProductPageKind.NotFound, ProductPage.NotFound.Kind);
        Assert.Equal((false, false, false, false, false), (ProductPage.NotFound.ShowsDraftBand, ProductPage.NotFound.ShowsPrice, ProductPage.NotFound.OffersCart, ProductPage.NotFound.IsIndexable, ProductPage.NotFound.IsNoLongerSold));
        Assert.Empty(ProductPage.NotFound.Variants);
        Assert.Null(ProductPage.NotFound.Name);
    }

    [Fact]
    public void OnlyAPublishedPage_OffersTheCartAndTheSearchEngines_OfEveryKind_KK6_KK7()
    {
        foreach (ProductPageKind kind in Enum.GetValues<ProductPageKind>())
        {
            ProductPage page = PageOf(kind);

            Assert.Equal(kind == ProductPageKind.Published, page.OffersCart);
            Assert.Equal(kind == ProductPageKind.Published, page.IsIndexable);
            Assert.Equal(kind is ProductPageKind.Published or ProductPageKind.DraftPreview, page.ShowsPrice);
        }
    }

    // ---- KK8, KK9: the results ----

    [Fact]
    public void TheRemovalResults_CarryTheCodesOfTheRegistry_KK5_KK8_KK9()
    {
        Assert.Equal(ErrorCode.NotFound, RemovalResult.NotFound.Code);
        Assert.Equal(ErrorCode.RecordVersionConflict, RemovalResult.VersionConflict.Code);
        Assert.Equal(ErrorCode.LastPublishedVariant, RemovalResult.LastPublishedVariant.Code);
        Guid missing = Guid.NewGuid();
        RemovalResult file = RemovalResult.FileRequired([missing]);
        Assert.Equal(ErrorCode.DigitalFileRequired, file.Code);
        Assert.Equal([missing], file.VariantsWithoutFile);
        Assert.All(new[] { RemovalResult.NotFound, RemovalResult.VersionConflict, RemovalResult.LastPublishedVariant, file }, r =>
        {
            Assert.True(r.IsRejected);
            Assert.False(r.IsDone);
        });
        Assert.All(new[] { RemovalResult.Done, RemovalResult.Unchanged }, r =>
        {
            Assert.False(r.IsRejected);
            Assert.Null(r.Code);
            Assert.Empty(r.VariantsWithoutFile);
        });
        Assert.True(RemovalResult.Done.IsDone);
        Assert.False(RemovalResult.Unchanged.IsDone);
    }

    [Fact]
    public void TheLastPublishedVariantRefusal_SendsTheAdministratorToTheProductsDraftOrArchive_InTheMoveAndInTheDeletion_KK9()
    {
        Assert.Equal([PublishStatus.Draft, PublishStatus.Archived], PublishChangeResult.LastPublishedVariant.SuggestedProductMoves);
        Assert.Equal([PublishStatus.Draft, PublishStatus.Archived], RemovalResult.LastPublishedVariant.SuggestedProductMoves);
        Assert.Equal(LastVariantGuidance.ProductMoves, PublishChangeResult.LastPublishedVariant.SuggestedProductMoves);
        Assert.Empty(PublishChangeResult.Changed.SuggestedProductMoves);
        Assert.Empty(PublishChangeResult.GateIncomplete([PublishCondition.Name]).SuggestedProductMoves);
        Assert.Empty(RemovalResult.NotFound.SuggestedProductMoves);
        Assert.Empty(RemovalResult.FileRequired([Guid.NewGuid()]).SuggestedProductMoves);
    }

    [Fact]
    public void AnOpenOrder_IsAnOrderOutsideTheTerminalStates_OfTheTransitionTable_KK8()
    {
        Assert.Equal([OrderStatus.Delivered, OrderStatus.Cancelled], TransitionTable.TerminalStatuses);
        foreach (OrderStatus status in Enum.GetValues<OrderStatus>())
        {
            Assert.Equal(TransitionTable.TerminalStatuses.Contains(status), TransitionTable.IsTerminal(status));
        }
    }

    // ---- KK8, KK9: the one place that deletes ----

    [Fact]
    public void ApplicationSource_DeletesAProductOrAVariantOnlyInTheRemovalUnit_KK8_KK9()
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
            findings.AddRange(CatalogDeletionScan.Scan(relative, File.ReadAllText(file)).Select(line => $"{relative}:{line}"));
        }

        Assert.Contains(CatalogDeletionScan.UnitFile, scanned);
        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("_db.Products.Remove(product);")]
    [InlineData("db.Variants.Remove(variant);")]
    [InlineData("_db.Variants.RemoveRange(variants);")]
    [InlineData("_db.Products.RemoveRange(list);")]
    [InlineData("await _db.Variants.Where(v => v.ProductId == id).ExecuteDeleteAsync(ct);")]
    [InlineData("_db.Products.ExecuteDelete();")]
    [InlineData("_db.Variants\n    .Where(v => v.Id == id)\n    .ExecuteDeleteAsync(ct);")]
    [InlineData("context.Set<Variant>().Remove(variant);")]
    [InlineData("_db.Set<Product>().RemoveRange(items);")]
    [InlineData("_db.Entry(product).State = EntityState.Deleted;")]
    [InlineData("_db.Remove(variant);")]
    [InlineData("context.RemoveRange(variants);")]
    [InlineData("Products.Remove(p);")]
    [InlineData("await _db.Database.ExecuteSqlRawAsync(\"DELETE FROM variants WHERE id = {0}\", id);")]
    [InlineData("await _db.Database.ExecuteSqlAsync($\"delete from public.products where id = {id}\", ct);")]
    [InlineData("await _db.Database.ExecuteSqlRawAsync(\"TRUNCATE TABLE \\\"products\\\" CASCADE\");")]
    public void Scan_OnEveryFormOfADeletion_ReportsIt_OutsideTheUnit(string text)
    {
        Assert.Single(CatalogDeletionScan.Scan("src/Shopfolio.Web/Modules/Catalog/Other.cs", text));
        Assert.Empty(CatalogDeletionScan.Scan(CatalogDeletionScan.UnitFile, text));
    }

    [Theory]
    [InlineData("_db.CartItems.RemoveRange(marked);")]
    [InlineData("_db.ProductImages.Remove(link);")]
    [InlineData("await expired.Where(a => ids.Contains(a.Id)).ExecuteDeleteAsync(cancel);")]
    [InlineData("// _db.Products.Remove(product);")]
    [InlineData("/// <c>_db.Variants.Remove(variant)</c>")]
    [InlineData("var products = _db.Products.Where(p => p.Id == id).ToList();")]
    [InlineData("await _db.Database.ExecuteSqlRawAsync(\"DELETE FROM cart_items WHERE id = {0}\", id);")]
    [InlineData("await _db.Database.ExecuteSqlRawAsync(\"UPDATE variants SET stock_quantity = 0 WHERE id = {0}\", id);")]
    public void Scan_OnWhatIsNotAProductOrVariantDeletion_ReportsNothing(string text)
    {
        Assert.Empty(CatalogDeletionScan.Scan("src/Shopfolio.Web/Modules/Catalog/Other.cs", text));
    }

    [Fact]
    public void Scan_OnAFileOfTheUnitsNameElsewhere_IsNotTheAllowedPlace()
    {
        Assert.Single(CatalogDeletionScan.Scan("src/Shopfolio.Web/Modules/Ordering/ProductRemoval.cs", "_db.Products.Remove(product);"));
    }

    [Fact]
    public void TheRemovalUnit_SavesOnlyThroughTheRecordSave_AndOpensNoTransactionOfItsOwn_KK8()
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), CatalogDeletionScan.UnitFile));

        Assert.DoesNotContain("SaveChanges", text, StringComparison.Ordinal);
        Assert.DoesNotContain("BeginTransaction", text, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Count(text, @"RecordSave\.SaveAsync\("));
    }

    private static ProductPage PageOf(ProductPageKind kind)
        => new(kind, Guid.NewGuid(), Name, null, null, kind == ProductPageKind.Archived ? [] : [new ProductPageVariant(Guid.NewGuid(), "SKU", 10m, false, PublishStatus.Published)]);
}

/// <summary>
/// The source scan of the deletion (T335 KK8, KK9; 02 3.7.7, 3.7.8): the statements that delete a product or a variant - a tracked removal, a bulk delete, a delete through
/// the set or the context, a state set to deleted - are in the removal unit's file and nowhere else, so the published product's gate (the last Published variant stays) and the
/// cart effect cannot be bypassed. The allowed place is the unit's full path, not a file name.
/// </summary>
internal static class CatalogDeletionScan
{
    public const string UnitFile = "src/Shopfolio.Web/Modules/Catalog/ProductRemoval.cs";

    private static readonly Regex OnTheSets = new(@"\b(Products|Variants)\b|\bSet<\s*(Product|Variant)\s*>", RegexOptions.Compiled);

    private static readonly Regex Deleting = new(@"\.\s*(Remove|RemoveRange|ExecuteDelete|ExecuteDeleteAsync)\s*\(", RegexOptions.Compiled);

    // A removal through the context itself names its record only by the variable: the scan reads a product's or a variant's variable by its name.
    private static readonly Regex OnTheContext = new(@"\b(_?db|_?context|_?dbContext)\s*\.\s*(Remove|RemoveRange)\s*\(\s*[^)]*(product|variant)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DeletedState = new(@"EntityState\s*\.\s*Deleted", RegexOptions.Compiled);

    private static readonly Regex NamesAProductOrAVariant = new(@"product|variant", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // A raw statement on the two tables: the table's name after DELETE FROM or TRUNCATE (quoted or not, schema-qualified or not).
    private static readonly Regex RawSql = new(@"\b(DELETE\s+FROM|TRUNCATE(\s+TABLE)?)\s+(ONLY\s+)?([\\""]*\w+[\\""]*\.)?[\\""]*(products|variants)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>The 1-based line a statement that deletes a product or a variant starts on, outside the unit's file; comment lines are not statements.</summary>
    public static IReadOnlyList<int> Scan(string relativePath, string text)
    {
        if (string.Equals(relativePath, UnitFile, StringComparison.Ordinal))
        {
            return [];
        }

        List<int> lines = [];
        string[] all = text.Split('\n');
        System.Text.StringBuilder statement = new();
        int start = 0;
        for (int i = 0; i < all.Length; i++)
        {
            string line = all[i].Trim();
            if (line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith('*') || line.StartsWith("/*", StringComparison.Ordinal))
            {
                continue;
            }

            if (statement.Length == 0)
            {
                start = i + 1;
            }

            statement.Append(' ').Append(line);
            if (line.EndsWith(';') || line.EndsWith('{') || line.EndsWith('}'))
            {
                if (IsDeletion(statement.ToString()))
                {
                    lines.Add(start);
                }

                statement.Clear();
            }
        }

        if (statement.Length > 0 && IsDeletion(statement.ToString()))
        {
            lines.Add(start);
        }

        return lines;
    }

    private static bool IsDeletion(string statement)
        => (OnTheSets.IsMatch(statement) && Deleting.IsMatch(statement))
            || OnTheContext.IsMatch(statement)
            || RawSql.IsMatch(statement)
            || (DeletedState.IsMatch(statement) && statement.Contains("Entry(", StringComparison.Ordinal) && NamesAProductOrAVariant.IsMatch(statement));
}
