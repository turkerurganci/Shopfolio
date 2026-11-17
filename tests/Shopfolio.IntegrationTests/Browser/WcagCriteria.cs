namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The 31 WCAG 2.2 level A criteria of the accessibility pass and the result each carries (T552 criterion 3, KK3; 12 12.2 (5); VAL-A776): met, or not applicable with its reason. The list is a second copy of the table of 12 12.2
/// on purpose - <see cref="AccessibilityRecordTests"/> reads the document and breaks when the two differ (a criterion, its status or its rows), so the record cannot drift from the protocol; what the table of the document does not hold
/// (the evidence of each criterion: the procedure of the manual audit or the test that checks it) is kept here and written to the record of the run.
/// </summary>
internal static class WcagCriteria
{
    /// <summary>The evidence of the criteria that every screen answers for itself: the headings of the manual audit and its page facts, read from the record of the screen.</summary>
    internal static readonly string[] ScreenEvidence = [.. AccessibilityAuditTests.Headings, FactsStep];

    internal const string FactsStep = "sayfa olguları";
    internal const string ScanStep = "otomatik tarama";
    internal const string SourceScan = "kaynak taraması (AccessibilitySourceScanTests)";
    internal const string ContextChange = "bağlam değişimi (AccessibilityCriteriaTests.AChangedValue...)";
    internal const string ConsistentHelp = "tutarlı yardım (AccessibilityCriteriaTests.TheHelpLink...)";
    internal const string RedundantEntry = "tekrarlı giriş (AccessibilityCriteriaTests.WhatTheVisitorTyped...)";

    /// <summary>One criterion: its number and name, whether it applies, the reason when it does not, the rows of 12 that carry it and the evidence that gives its result.</summary>
    internal sealed record Criterion(string Id, string Name, bool Applies, string Reason, string[] Rows, string[] Evidence);

    private const string NoMedia = "ürün ses ve görüntü oynatmaz (sayfa olguları: ortam öğesi yok)";

    private static Criterion Met(string id, string name, string[] rows, params string[] evidence) => new(id, name, true, string.Empty, rows, evidence);

    private static Criterion NotApplicable(string id, string name, string reason, string[] rows, params string[] evidence) => new(id, name, false, reason, rows, evidence);

    internal static readonly IReadOnlyList<Criterion> All =
    [
        Met("1.1.1", "Metin dışı içerik", ["VAL-A344"], AccessibilityAuditTests.Name, ScanStep),
        NotApplicable("1.2.1", "Yalnız ses ve yalnız görüntü", NoMedia, ["VAL-A789"], FactsStep),
        NotApplicable("1.2.2", "Altyazı (kayıtlı)", NoMedia, ["VAL-A789"], FactsStep),
        NotApplicable("1.2.3", "Sesli betimleme ya da ortam alternatifi", NoMedia, ["VAL-A789"], FactsStep),
        Met("1.3.1", "Bilgi ve ilişkiler", ["VAL-A777", "VAL-A347"], AccessibilityAuditTests.Name, AccessibilityAuditTests.Error, FactsStep, ScanStep),
        Met("1.3.2", "Anlamlı sıra", ["VAL-A777"], AccessibilityAuditTests.FocusOrder, AccessibilityAuditTests.Zoom),
        Met("1.3.3", "Duyusal özellikler", ["VAL-A778"], FactsStep),
        Met("1.4.1", "Renk kullanımı", ["VAL-A348"], AccessibilityAuditTests.Error, ScanStep),
        NotApplicable("1.4.2", "Ses denetimi", NoMedia, ["VAL-A789"], FactsStep),
        Met("2.1.1", "Klavye", ["VAL-A345"], AccessibilityAuditTests.Keyboard),
        Met("2.1.2", "Klavye tuzağı yok", ["VAL-A345"], AccessibilityAuditTests.Keyboard),
        NotApplicable("2.1.4", "Tek karakterli kısayol", "ürün tek karakterli kısayol tanımlamaz (ortak betik yalnız Escape ve ok tuşlarını işler)", ["VAL-A789"], SourceScan),
        Met("2.2.1", "Zamanlama ayarlanabilir", ["VAL-A788"], FactsStep),
        Met("2.2.2", "Durdur, duraklat, gizle", ["VAL-A788"], FactsStep),
        Met("2.3.1", "Üç parlama", ["VAL-A788"], FactsStep, SourceScan),
        Met("2.4.1", "Blokları atla", ["VAL-A779"], AccessibilityAuditTests.Keyboard, ScanStep),
        Met("2.4.2", "Sayfa başlığı", ["VAL-A780"], FactsStep, ScanStep),
        Met("2.4.3", "Odak sırası", ["VAL-A781", "VAL-A346"], AccessibilityAuditTests.FocusOrder),
        Met("2.4.4", "Bağlantı amacı", ["VAL-A782"], AccessibilityAuditTests.Name, ScanStep),
        Met("2.5.1", "İşaretçi hareketleri", ["VAL-A787"], SourceScan),
        Met("2.5.2", "İşaretçi iptali", ["VAL-A787"], SourceScan),
        Met("2.5.3", "Adda etiket", ["VAL-A787"], AccessibilityAuditTests.Name, ScanStep),
        Met("2.5.4", "Hareket etkinleştirme", ["VAL-A787"], SourceScan),
        Met("3.1.1", "Sayfanın dili", ["VAL-A783"], FactsStep, ScanStep),
        Met("3.2.1", "Odakta", ["VAL-A784"], AccessibilityAuditTests.Keyboard),
        Met("3.2.2", "Girdide", ["VAL-A784"], ContextChange),
        Met("3.2.6", "Tutarlı yardım", ["VAL-A341"], ConsistentHelp),
        Met("3.3.1", "Hatanın tanımlanması", ["VAL-A785"], AccessibilityAuditTests.Error),
        Met("3.3.2", "Etiket veya yönerge", ["VAL-A347"], AccessibilityAuditTests.Name, ScanStep),
        Met("3.3.7", "Tekrarlı giriş", ["VAL-A342"], RedundantEntry),
        Met("4.1.2", "Ad, rol, değer", ["VAL-A786"], AccessibilityAuditTests.Name, ScanStep),
    ];
}
