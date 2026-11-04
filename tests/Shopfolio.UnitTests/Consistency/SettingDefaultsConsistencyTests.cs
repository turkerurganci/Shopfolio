using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Migrations;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the start-up data's setting values equal to the firm setting table of <c>02 §11.1</c> (T119 KK7; 06 8.1.1; 09 21.4 item 1): every
/// row whose layer is exactly "Firma ayarı" has its bold default in the start-up data migration and no other parameter is there. "Boş" is an
/// empty column; a number is the column's value. Runs in the Lint step, not in the Unit step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class SettingDefaultsConsistencyTests
{
    [Fact]
    public void StartupData_TakesEveryFirmSettingDefaultOf02_11_1()
    {
        Dictionary<string, decimal?> fromDocument = FirmSettingDefaults();
        Dictionary<string, decimal?> fromMigration = InsertStartupData.SettingDefaults.ToDictionary(s => s.Parameter, s => s.Value, StringComparer.Ordinal);

        Assert.Equal(["P-1", "P-10", "P-3", "P-4", "P-5", "P-7", "P-8"], fromDocument.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(fromDocument.Keys.Order(StringComparer.Ordinal), fromMigration.Keys.Order(StringComparer.Ordinal));
        Assert.All(fromDocument, pair => Assert.Equal(pair.Value, fromMigration[pair.Key]));
    }

    [Fact]
    public void StartupData_WritesEachSettingColumnOnce()
    {
        string[] columns = InsertStartupData.SettingDefaults.Select(s => s.Column)
            .Concat(InsertStartupData.MenuNames.Select(m => m.Column))
            .ToArray();

        Assert.Equal(columns.Length, columns.Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>The rows of 02 11.1 whose layer is "Firma ayarı" and whose default is bold: parameter number to value ("Boş" is null).</summary>
    private static Dictionary<string, decimal?> FirmSettingDefaults()
    {
        string requirements = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "02_PRODUCT_REQUIREMENTS.md"));
        int start = requirements.IndexOf("### 11.1 ", StringComparison.Ordinal);
        int end = requirements.IndexOf("### 11.2 ", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start, "02 §11.1 was not found.");

        Dictionary<string, decimal?> defaults = new(StringComparer.Ordinal);
        foreach (Match row in FirmSettingRow().Matches(requirements[start..end]))
        {
            string text = row.Groups["value"].Value.Trim();
            Match number = LeadingNumber().Match(text);
            decimal? value = text == "Boş"
                ? null
                : number.Success
                    ? decimal.Parse(number.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture)
                    : throw new InvalidOperationException($"02 §11.1 {row.Groups["id"].Value}: the default '{text}' is neither 'Boş' nor a number.");
            defaults.Add(row.Groups["id"].Value, value);
        }

        return defaults;
    }

    // | P-5 | <parameter> | Firma ayarı | **2 iş günü** ...
    [GeneratedRegex(@"^\|\s*(?<id>P-\d+)\s*\|[^|]*\|\s*Firma ayarı\s*\|\s*\*\*(?<value>[^*]+)\*\*", RegexOptions.Multiline)]
    private static partial Regex FirmSettingRow();

    [GeneratedRegex(@"^%?(\d+(?:,\d+)?)")]
    private static partial Regex LeadingNumber();
}
