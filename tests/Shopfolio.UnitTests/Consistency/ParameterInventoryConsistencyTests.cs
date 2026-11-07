using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the parameter inventory of the code equal to the table of <c>02 §11</c> (T381 criterion 10, KK8, KK10; 09 21.4 item 1, 3.5 item 1): every P row of
/// <c>§11.1</c> and <c>§11.2</c> is in <see cref="ParameterInventory"/> under the layer the table gives it and nothing else is; every product constant of
/// <see cref="ProductConstants"/> belongs to exactly one P row and every value that the row writes as a bold number is the constant's value in the row's unit; the fences of
/// the firm settings are the "Çit" column's. The attempt limits' own numbers are compared by <c>AttemptLimitsConsistencyTests</c>. The expected values are read from the
/// document, never typed in the test. Runs in the Lint step, not in the Unit step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class ParameterInventoryConsistencyTests
{
    private sealed record Row(string Id, string Parameter, string Layer, string Value, string Fence);

    private static string Document() =>
        File.ReadAllText(Assert.Single(Directory.GetFiles(Path.Combine(RepositoryRoot.Find(), "Docs"), "02_*.md"))).ReplaceLineEndings("\n");

    // The rows of the two tables of 02 11: | P-n | parameter | layer | value | fence | where | source |
    private static List<Row> Rows()
    {
        string document = Document();
        int start = document.IndexOf("## 11. ", StringComparison.Ordinal);
        int end = document.IndexOf("### 11.3 ", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "02 §11 was not found.");

        List<Row> rows = [];
        foreach (string line in document[start..end].Split('\n').Where(l => l.StartsWith("| P-", StringComparison.Ordinal)))
        {
            string[] cells = line.Split('|', StringSplitOptions.None).Select(c => c.Trim()).ToArray();
            Assert.True(cells.Length >= 8, "A parameter row has changed shape: " + line);
            rows.Add(new Row(cells[1], cells[2], cells[3], cells[4], cells[5]));
        }

        return rows;
    }

    private static ParameterLayer? LayerOf(Row row)
    {
        if (row.Parameter.StartsWith("*Kaldırıldı*", StringComparison.Ordinal))
        {
            return null;
        }

        if (row.Layer.StartsWith("Firma ayarı", StringComparison.Ordinal))
        {
            return ParameterLayer.CompanySetting;
        }

        return row.Layer == "Ürün sabiti"
            ? ParameterLayer.ProductConstant
            : throw new InvalidOperationException($"02 §11 {row.Id}: the layer '{row.Layer}' is not known.");
    }

    private static Dictionary<string, FieldInfo> ConstantFields() =>
        typeof(ProductConstants).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .ToDictionary(f => f.Name, StringComparer.Ordinal);

    [Fact]
    public void EveryRowOfTheDocument_IsInTheInventory_UnderTheSameLayer_AndNothingElseIsInIt()
    {
        List<Row> rows = Rows();

        Assert.Equal(49, rows.Count);
        Assert.Equal(
            Enumerable.Range(1, 49).Select(n => $"P-{n}").Order(StringComparer.Ordinal),
            ParameterInventory.Entries.Select(e => e.Id).Order(StringComparer.Ordinal));
        Assert.Equal(
            rows.Select(r => r.Id).Order(StringComparer.Ordinal),
            ParameterInventory.Entries.Select(e => e.Id).Order(StringComparer.Ordinal));
        foreach (Row row in rows)
        {
            Assert.True(LayerOf(row) == ParameterInventory.Of(row.Id).Layer, $"{row.Id}: layer of the document and of the code differ");
        }

        Assert.Equal(10, ParameterInventory.Entries.Count(e => e.Layer == ParameterLayer.CompanySetting));
        Assert.Equal(37, ParameterInventory.Entries.Count(e => e.Layer == ParameterLayer.ProductConstant));
        Assert.Equal(["P-11", "P-2"], ParameterInventory.Entries.Where(e => e.IsRemoved).Select(e => e.Id).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheTextOfSection11_SaysTenFirmSettingsAndThirtySevenProductConstants_AndNamesTheRemovedIds()
    {
        string document = Document();

        Assert.Contains("on firma ayarı ile otuz yedi ürün sabiti, toplam kırk yedi parametre", document, StringComparison.Ordinal);
        Assert.Contains("P-11 (duyuru metni) envanterden çıkmıştır", document, StringComparison.Ordinal);
        Assert.Contains("P-2 (kargo KDV oranı) çıkmıştır", document, StringComparison.Ordinal);
        Assert.Contains("Kimlikler yeniden sıralanmaz", document, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryProductConstantOfTheCode_BelongsToExactlyOneRow_AndEveryRowNamesExistingConstants()
    {
        Dictionary<string, FieldInfo> fields = ConstantFields();
        string[] named = [.. ParameterInventory.Entries.SelectMany(e => e.Constants).Select(c => c.Name)];

        Assert.Equal(named.Length, named.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(fields.Keys.Order(StringComparer.Ordinal), named.Order(StringComparer.Ordinal));
        Assert.All(
            ParameterInventory.Entries.Where(e => e.Layer == ParameterLayer.ProductConstant),
            e => Assert.NotEmpty(e.Constants));
        Assert.All(
            ParameterInventory.Entries.Where(e => e.Layer != ParameterLayer.ProductConstant),
            e => Assert.Empty(e.Constants));
    }

    [Fact]
    public void EveryConstantValueTheRowWritesInBold_IsTheConstantsValueInTheRowsUnit()
    {
        Dictionary<string, FieldInfo> fields = ConstantFields();
        List<string> checkedRows = [];
        foreach (Row row in Rows().Where(r => LayerOf(r) == ParameterLayer.ProductConstant))
        {
            ParameterEntry entry = ParameterInventory.Of(row.Id);
            if (entry.Constants.All(c => c.Unit == ParameterUnit.AttemptLimit))
            {
                // The threshold and the window of an attempt limit are compared by AttemptLimitsConsistencyTests.
                continue;
            }

            string[] tokens = [.. Bold().Matches(row.Value).Select(m => m.Groups[1].Value.Trim())];
            Assert.True(tokens.Length == entry.Constants.Count, $"{row.Id}: the row writes {tokens.Length} bold value(s), the inventory names {entry.Constants.Count} constant(s)");
            for (int i = 0; i < tokens.Length; i++)
            {
                ParameterConstant constant = entry.Constants[i];
                object value = fields[constant.Name].GetRawConstantValue()!;
                if (constant.Unit == ParameterUnit.Text)
                {
                    Assert.Equal(tokens[i], (string)value);
                    continue;
                }

                (string family, decimal expected) = Parse(row.Id, tokens[i]);
                (string codeFamily, decimal actual) = Canonical(constant.Unit, Convert.ToDecimal(value, CultureInfo.InvariantCulture));
                Assert.True(family == codeFamily && expected == actual, $"{row.Id} {constant.Name}: the document says '{tokens[i]}', the code is {value} ({constant.Unit})");
            }

            checkedRows.Add(row.Id);
        }

        // The check really ran over the rows that are not attempt limits: 28 product-constant rows.
        Assert.Equal(28, checkedRows.Count);
    }

    [Fact]
    public void TheFencesOfTheFirmSettings_AreTheFenceColumnOfTheDocument()
    {
        Dictionary<string, Row> rows = Rows().ToDictionary(r => r.Id, StringComparer.Ordinal);
        string[] firm = [.. ParameterInventory.Entries.Where(e => e.Layer == ParameterLayer.CompanySetting).Select(e => e.Id)];

        foreach (string id in firm)
        {
            // P-6 has the fence of P-5 ("P-5'in üst çiti"); the cell names it.
            Row row = id == "P-6" ? rows["P-5"] : rows[id];
            Assert.True(id != "P-6" || rows["P-6"].Fence.Contains("P-5'in üst çiti", StringComparison.Ordinal), "P-6 no longer takes P-5's fence");
            int? lower = Number(Lower().Match(row.Fence));
            int? upper = Number(Upper().Match(row.Fence));
            FenceBounds code = FirmSettingFences.BoundsOf(id);

            // The shipping period starts at zero by itself (0 business days is "ships the same day"); the table writes no lower fence for it.
            int? expectedLower = lower ?? (id is "P-5" or "P-6" ? 0 : null);
            Assert.True(code.Lower == expectedLower, $"{id} lower fence: document {lower}, code {code.Lower}");
            Assert.True(code.Upper == upper, $"{id} upper fence: document {upper}, code {code.Upper}");
        }
    }

    [Fact]
    public void TheInstallationSettingsOfTheCode_AreTheOnesSection10_1_3Names()
    {
        string document = Document();
        string layers = document[document.IndexOf("**10.1.3 ", StringComparison.Ordinal)..];
        layers = layers[..layers.IndexOf('\n', StringComparison.Ordinal)];

        Assert.Contains("alan adı", layers, StringComparison.Ordinal);
        Assert.Contains("e-posta gönderim kimliği", layers, StringComparison.Ordinal);
        Assert.Contains("ödeme sağlayıcı anahtarları", layers, StringComparison.Ordinal);
        Assert.Contains("Google uygulamasının kimlik bilgileri", layers, StringComparison.Ordinal);
        Assert.Equal(
            ["Domain", "EmailSendingIdentity", "GoogleApplication", "PaymentProviderKeys", "SenderAddress"],
            ParameterInventory.InstallationSettings.Select(s => s.Name).Order(StringComparer.Ordinal));
        Assert.Equal(3, ParameterInventory.InstallationSettings.Count(s => s.IsRequired));
    }

    private static int? Number(Match match) => match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : null;

    // "24 saat" -> time in minutes; "6 karakter"; "10 MB" -> bytes; "2" -> a count; "Günde bir" -> once a day.
    private static (string Family, decimal Value) Parse(string id, string token)
    {
        if (token == "Günde bir")
        {
            return ("PerDay", 1);
        }

        Match match = NumberAndWord().Match(token);
        Assert.True(match.Success, $"{id}: the bold value '{token}' is not a number with a unit");
        decimal number = decimal.Parse(match.Groups["n"].Value, CultureInfo.InvariantCulture);
        return match.Groups["w"].Value switch
        {
            "dakika" => ("Time", number),
            "saat" => ("Time", number * 60),
            "gün" => ("Time", number * 1440),
            "ay" => ("Months", number),
            "yıl" => ("Years", number),
            "karakter" => ("Characters", number),
            "MB" => ("Bytes", number * 1024 * 1024),
            "CSS pikseli" => ("CssPixels", number),
            "seviye" or "" => ("Count", number),
            string other => throw new InvalidOperationException($"{id}: the unit '{other}' of '{token}' is not known to this check."),
        };
    }

    private static (string Family, decimal Value) Canonical(ParameterUnit unit, decimal value) => unit switch
    {
        ParameterUnit.Minutes => ("Time", value),
        ParameterUnit.Hours => ("Time", value * 60),
        ParameterUnit.Days => ("Time", value * 1440),
        ParameterUnit.Months => ("Months", value),
        ParameterUnit.Years => ("Years", value),
        ParameterUnit.Characters => ("Characters", value),
        ParameterUnit.Bytes => ("Bytes", value),
        ParameterUnit.CssPixels => ("CssPixels", value),
        ParameterUnit.PerDay => ("PerDay", value),
        ParameterUnit.Count => ("Count", value),
        _ => throw new InvalidOperationException($"The unit {unit} has no canonical form."),
    };

    [GeneratedRegex(@"\*\*([^*]+)\*\*")]
    private static partial Regex Bold();

    [GeneratedRegex(@"^(?<n>\d+) ?(?<w>.*)$")]
    private static partial Regex NumberAndWord();

    [GeneratedRegex(@"(?:[Aa]lt çit|[Ee]n az) \*\*(\d+)")]
    private static partial Regex Lower();

    [GeneratedRegex(@"[Üü]st çit \*\*(\d+)")]
    private static partial Regex Upper();
}
