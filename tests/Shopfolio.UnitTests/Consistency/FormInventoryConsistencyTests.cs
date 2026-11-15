using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality check of the form inventory (T547; 04 7.1, 7.2; 09 3.5 item 1, 19.2 item 3, 21.4 item 1): every form is a block and every field a row numbered without a gap; the customer side's forms are on the
/// customer side's screens and the panel's under <c>/panel</c>; a rule is an id of <c>02</c> or of 04 7.3 and writes no value; a product constant a rule points to is read by the code from
/// <c>ProductConstants</c> and no length attribute writes the number. The checks that must fail read a changed copy of the document or of the sources, never the files (09 19.2 item 3). Reads repository files
/// only; runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed class FormInventoryConsistencyTests
{
    private static readonly Lazy<string> Document = new(StateRoleDocument.Load);

    private static readonly Lazy<string> Requirements = new(() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "02_PRODUCT_REQUIREMENTS.md")).ReplaceLineEndings("\n"));

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Sources = new(() => NavigationSites.Load(RepositoryRoot.Find()));

    private static IReadOnlyList<string> Problems(string? document = null, IReadOnlyDictionary<string, string>? sources = null)
        => FormInventoryCheck.Problems(document ?? Document.Value, sources ?? Sources.Value, Requirements.Value);

    private static string Changed(string from, string to)
    {
        string changed = Document.Value.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(Document.Value, changed);
        return changed;
    }

    [Fact]
    public void FormInventory_EqualsTheDocumentsAndTheCode()
    {
        IReadOnlyList<string> problems = Problems();

        Assert.True(problems.Count == 0, string.Join('\n', problems));
    }

    [Fact]
    public void FieldInventory_EqualsTheDocumentsAndTheCode()
    {
        IReadOnlyList<string> problems = FormFieldCheck.Problems(Document.Value, Sources.Value);

        Assert.True(problems.Count == 0, string.Join('\n', problems));
    }

    // KK6: a field's row without an entry, a field that leaves the code, a requirement or a rule changed in the document.
    [Fact]
    public void Field_DeletedFromTheCode_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        Assert.Contains("MinPrice", sources["Pages/Storefront/CategoryPage.cs"], StringComparison.Ordinal);
        sources["Pages/Storefront/CategoryPage.cs"] = sources["Pages/Storefront/CategoryPage.cs"].Replace("MinPrice", "LowestPrice", StringComparison.Ordinal);

        Assert.Contains(FormFieldCheck.Problems(Document.Value, sources), p => p.StartsWith("FieldNotInCode:7.1.1.1", StringComparison.Ordinal));
    }

    [Fact]
    public void Field_DeletedFromTheDocument_IsReported()
    {
        string changed = Changed("| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.4) | Alan mesajı | L-4 |\n", string.Empty);

        Assert.Contains(FormFieldCheck.Problems(changed, Sources.Value), p => p.StartsWith("FieldRowOnlyInCode:7.1.3.3", StringComparison.Ordinal));
    }

    [Fact]
    public void Field_AddedToTheDocument_IsReported()
    {
        string changed = Changed("| 7.1.3.6 | Görünmez tuzak alan |", "| 7.1.3.7 | Yeni alan | İsteğe bağlı | Kısa metin | Alan mesajı | — |\n| 7.1.3.6 | Görünmez tuzak alan |");

        Assert.Contains(FormFieldCheck.Problems(changed, Sources.Value), p => p.StartsWith("FieldRowOnlyInDocument:7.1.3.7", StringComparison.Ordinal));
    }

    [Fact]
    public void Requirement_ChangedInTheDocument_IsReported()
    {
        string changed = Changed("| 7.1.3.3 | Telefon | İsteğe bağlı |", "| 7.1.3.3 | Telefon | Zorunlu |");

        Assert.Contains(FormFieldCheck.Problems(changed, Sources.Value), p => p.StartsWith("RequirementDiffers:7.1.3.3", StringComparison.Ordinal));
    }

    [Fact]
    public void Rule_ChangedInTheDocument_IsReported()
    {
        string changed = Changed("| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.4) |", "| 7.1.3.3 | Telefon | İsteğe bağlı | Kısa metin |");

        Assert.Contains(FormFieldCheck.Problems(changed, Sources.Value), p => p.StartsWith("RuleDiffers:7.1.3.3", StringComparison.Ordinal));
    }

    [Fact]
    public void Limit_ChangedInTheDocument_IsReported()
    {
        string changed = Changed("| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.4) | Alan mesajı | L-4 |", "| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.4) | Alan mesajı | L-2 |");

        Assert.Contains(FormFieldCheck.Problems(changed, Sources.Value), p => p.StartsWith("LimitDiffers:7.1.3.3", StringComparison.Ordinal));
    }

    [Fact]
    public void FieldMarkedRequiredInTheCode_ThatTheRowCallsOptional_IsReported()
    {
        string changed = Changed("| 7.1.6.2 | E-posta | Zorunlu |", "| 7.1.6.2 | E-posta | İsteğe bağlı |");

        Assert.Contains(FormFieldCheck.Problems(changed, Sources.Value), p => p.StartsWith("RequirementContradictsCode:7.1.6.2", StringComparison.Ordinal));
    }

    [Fact]
    public void FieldWithAnAttribute_WhoseRuleTheRowDoesNotName_IsReported()
    {
        string changed = Changed("| 7.1.6.2 | E-posta | Zorunlu | E-posta (7.3.2.1)", "| 7.1.6.2 | E-posta | Zorunlu | Kısa metin");

        Assert.Contains(FormFieldCheck.Problems(changed, Sources.Value), p => p.StartsWith("RuleMissesAttribute:7.1.6.2", StringComparison.Ordinal));
    }

    [Fact]
    public void Document_HasBothFamiliesOfForms()
    {
        IReadOnlyList<FormBlock> forms = StateRoleDocument.Forms(Document.Value);

        Assert.Contains(forms, f => f.Block.StartsWith("7.1.", StringComparison.Ordinal));
        Assert.Contains(forms, f => f.Block.StartsWith("7.2.", StringComparison.Ordinal));
        Assert.All(forms, f => Assert.NotEmpty(f.Rows));
    }

    // KK6: a field row deleted, a rule changed to another id.
    [Fact]
    public void FieldRow_DeletedFromTheDocument_LeavesAGapInTheNumbering()
    {
        string changed = Changed("| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.4) | Alan mesajı | L-4 |\n", string.Empty);

        Assert.Contains(Problems(document: changed), p => p.StartsWith("FieldRowId:7.1.3", StringComparison.Ordinal));
    }

    [Fact]
    public void FormBlock_DeletedFromTheDocument_LeavesAGapInTheNumbering()
    {
        string changed = Changed("**7.1.4 E-12 — sepetin adet alanı** (5.12.12).", "**7.1.14 E-12 — sepetin adet alanı** (5.12.12).");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("FormNumbering:7.1", StringComparison.Ordinal));
    }

    [Fact]
    public void Rule_PointingToAnUnknownFieldType_IsReported()
    {
        string changed = Changed("| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.4) |", "| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.40) |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("RuleIdUnknown:7.1.3.3", StringComparison.Ordinal));
    }

    [Fact]
    public void Rule_PointingToAnUnknownParameter_IsReported()
    {
        string changed = Changed("| 7.2.4.2 | Ad | Zorunlu | Kısa metin, P-27 (7.3.2.14) |", "| 7.2.4.2 | Ad | Zorunlu | Kısa metin, P-97 (7.3.2.14) |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("RuleIdUnknown:7.2.4.2", StringComparison.Ordinal) && p.Contains("P-97", StringComparison.Ordinal));
    }

    [Fact]
    public void Limit_PointingToAnUnknownAttemptLimit_IsReported()
    {
        string changed = Changed("| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.4) | Alan mesajı | L-4 |", "| 7.1.3.3 | Telefon | İsteğe bağlı | Telefon (7.3.2.4) | Alan mesajı | L-44 |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("RuleIdUnknown:7.1.3.3", StringComparison.Ordinal) && p.Contains("L-44", StringComparison.Ordinal));
    }

    [Fact]
    public void Requirement_OfAnotherWord_IsReported()
    {
        string changed = Changed("| 7.1.3.3 | Telefon | İsteğe bağlı |", "| 7.1.3.3 | Telefon | Gönüllü |");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("RequirementWord:7.1.3.3", StringComparison.Ordinal));
    }

    // KK7: a number written instead of an id.
    [Theory]
    [InlineData("| 7.2.4.2 | Ad | Zorunlu | Kısa metin, P-27 (7.3.2.14) |", "| 7.2.4.2 | Ad | Zorunlu | Kısa metin, en çok 150 karakter (7.3.2.14) |")]
    [InlineData("| En fazla P-21 boyut (", "| En fazla 2 boyut (")]
    [InlineData("yüzde (7.3.2.9); sıfırdan büyük; yüzdesel kuponun üst sınırı `02 §3.10.4`'tedir |", "yüzde (7.3.2.9); sıfırdan büyük; yüzdesel %100'e ulaşamaz |")]
    [InlineData("kutularının işareti kalkar (§2.12.7.3;", "kutularının işareti iki kez kalkar (§2.12.7.3;")]
    public void Rule_WritingAValue_IsReported(string from, string to)
    {
        Assert.Contains(Problems(document: Changed(from, to)), p => p.StartsWith("RuleWritesValue", StringComparison.Ordinal));
    }

    [Fact]
    public void ProductConstant_NoValidationReads_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        foreach (string file in sources.Where(s => s.Value.Contains("ProductConstants.OptionDimensionMax", StringComparison.Ordinal) && !s.Key.EndsWith("ParameterInventory.cs", StringComparison.Ordinal)).Select(s => s.Key).ToArray())
        {
            sources[file] = sources[file].Replace("ProductConstants.OptionDimensionMax", "2", StringComparison.Ordinal);
        }

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("ProductConstantNotRead:P-21", StringComparison.Ordinal));
    }

    [Fact]
    public void LengthAttribute_WritingTheNumber_IsReported()
    {
        Dictionary<string, string> sources = new(Sources.Value, StringComparer.Ordinal);
        Assert.Contains("[TextTooLong(ProductConstants.ShortTextMaxLength)]", sources["Pages/Panel/Categories.cshtml.cs"], StringComparison.Ordinal);
        sources["Pages/Panel/Categories.cshtml.cs"] = sources["Pages/Panel/Categories.cshtml.cs"].Replace("[TextTooLong(ProductConstants.ShortTextMaxLength)]", "[TextTooLong(150)]", StringComparison.Ordinal);

        Assert.Contains(Problems(sources: sources), p => p.StartsWith("AttributeLimitIsANumber: Pages/Panel/Categories.cshtml.cs", StringComparison.Ordinal));
    }

    // KK8: the panel's forms are the panel screens' and the customer side's are not.
    [Fact]
    public void PanelForm_OnACustomerScreen_IsReported()
    {
        string changed = Changed("**7.2.5 E-34 — kategori**", "**7.2.5 E-04 — kategori**");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("FormScreenSide:7.2.5", StringComparison.Ordinal));
    }

    [Fact]
    public void CustomerForm_OnAPanelScreen_IsReported()
    {
        string changed = Changed("**7.1.3 E-10 — iletişim formu**", "**7.1.3 E-44 — iletişim formu**");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("FormScreenSide:7.1.3", StringComparison.Ordinal));
    }

    [Fact]
    public void Form_OfAnUnknownScreen_IsReported()
    {
        string changed = Changed("**7.1.3 E-10 — iletişim formu**", "**7.1.3 E-99 — iletişim formu**");

        Assert.Contains(Problems(document: changed), p => p.StartsWith("FormScreenUnknown:7.1.3", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("Büyük-küçük harf ayrımsız", "")]
    [InlineData("Yüzde (7.3.2.9; `02 §3.9.1`) — tarih (5.2.7; OB-12; K-799) 7.3.2.14", "")]
    [InlineData("P-27'nin çiti, L-4, Z-18", "")]
    [InlineData("en çok 150 karakter", "150")]
    [InlineData("yüzdesel %100'e ulaşamaz", "%100")]
    public void Values_AreTheNumbersThatAreNotIds(string cell, string expected)
    {
        Assert.Equal(expected, FormInventoryCheck.Values(cell));
    }
}
