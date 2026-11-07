using System.Reflection;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scans that keep the three parameter layers apart (T226; 05 8.4.1, MK-71; 09 3.5 items 1-3). Two scans over text:
/// (1) a settings class (<c>...Options</c>) must not carry a member that has the name of a product constant or of a firm setting
/// (<c>CompanySettings</c> property): a value has one home, and the layers do not mix; (2) the settings template (<c>deploy/.env.example</c>)
/// must not list a key that names a product constant or a secret setting (a secret setting comes from a secret file only, 05 8.4.3).
/// Names are compared by letters and digits only, ignoring case, so <c>CategoryLevelMax</c> and <c>CATEGORY_LEVEL_MAX</c> are one name.
/// </summary>
internal static partial class SettingLayerScan
{
    public const string OptionsMemberNamesAnotherLayer = "settings class member has the name of a product constant or a firm setting";

    public const string TemplateKeyNamesProductConstant = "settings template key names a product constant";

    public const string TemplateKeyIsSecretSetting = "settings template lists a secret setting";

    public const string FormFieldNamesProductConstant = "panel form field has the name of a product constant";

    /// <summary>The normalized names of the constants (fields that are literals) of a class.</summary>
    public static HashSet<string> ConstantNames(Type constantsClass)
    {
        return constantsClass.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => Normalize(field.Name))
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>The normalized names of the public instance properties of a class, without the listed ones.</summary>
    public static HashSet<string> PropertyNames(Type type, params string[] except)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .Where(name => !except.Contains(name, StringComparer.Ordinal))
            .Select(Normalize)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>True when the text declares a settings class (a class whose name ends with <c>Options</c>).</summary>
    public static bool DeclaresOptionsClass(string text) => OptionsClass().IsMatch(text);

    /// <summary>Findings of scan (1): the rule and the 1-based line of each member that has a forbidden name.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanOptionsSource(string text, IReadOnlySet<string> forbiddenNames)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            Match member = PublicMember().Match(lines[i]);
            if (member.Success && forbiddenNames.Contains(Normalize(member.Groups["name"].Value)))
            {
                findings.Add((OptionsMemberNamesAnotherLayer, i + 1));
            }
        }

        return findings;
    }

    /// <summary>Findings of scan (2): the rule and the 1-based line of each template key that names a product constant or a secret setting.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanSettingsTemplate(
        string templateText,
        IReadOnlySet<string> productConstantNames,
        IReadOnlySet<string> secretSettingKeys)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = templateText.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            Match assignment = TemplateAssignment().Match(lines[i]);
            if (!assignment.Success)
            {
                continue;
            }

            string[] segments = assignment.Groups["key"].Value.Split("__", StringSplitOptions.RemoveEmptyEntries);
            if (segments.Any(segment => productConstantNames.Contains(Normalize(segment))))
            {
                findings.Add((TemplateKeyNamesProductConstant, i + 1));
            }

            if (secretSettingKeys.Contains(Normalize(string.Concat(segments))))
            {
                findings.Add((TemplateKeyIsSecretSetting, i + 1));
            }
        }

        return findings;
    }

    /// <summary>
    /// Findings of scan (3), the panel's forms (T381 criterion 7; 02 11.2): a form field - a bound page model member or a markup field (<c>name</c>, <c>id</c> or
    /// <c>asp-for</c>) - whose last name segment is the name of a product constant. A product constant is not a setting, so the panel has no field for it.
    /// Returns the rule and the 1-based line of each finding.
    /// </summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanPanelFormSource(string text, IReadOnlySet<string> productConstantNames)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        bool bindPending = false;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            bool boundHere = BindPropertyAttribute().IsMatch(line);
            Match member = PublicProperty().Match(line);
            bool flagged = false;
            if (member.Success && (boundHere || bindPending) && productConstantNames.Contains(Normalize(member.Groups["name"].Value)))
            {
                flagged = true;
            }

            foreach (Match field in MarkupField().Matches(line))
            {
                string value = field.Groups["name"].Value;
                if (productConstantNames.Contains(Normalize(value[(value.LastIndexOf('.') + 1)..])))
                {
                    flagged = true;
                }
            }

            if (flagged)
            {
                findings.Add((FormFieldNamesProductConstant, i + 1));
            }

            bindPending = boundHere && !member.Success;
        }

        return findings;
    }

    /// <summary>The comparison form of a name: letters and digits, upper case.</summary>
    public static string Normalize(string name) => new string([.. name.Where(char.IsLetterOrDigit)]).ToUpperInvariant();

    [GeneratedRegex(@"\bclass\s+\w+Options\b", RegexOptions.CultureInvariant)]
    private static partial Regex OptionsClass();

    // public int Name { get; set; }  /  public const string Name = ...;  /  public static readonly ... Name = ...;
    [GeneratedRegex(@"^\s*public\s+(?:const\s+|static\s+|readonly\s+)*[\w<>\[\]?,. ]+?\s+(?<name>\w+)\s*(?:\{|=)", RegexOptions.CultureInvariant)]
    private static partial Regex PublicMember();

    [GeneratedRegex(@"\[\s*BindProperty\b", RegexOptions.CultureInvariant)]
    private static partial Regex BindPropertyAttribute();

    // public int Name { get; set; }  (after the [BindProperty] attribute, on the same line or the line before)
    [GeneratedRegex(@"\bpublic\s+[\w<>\[\]?,. ]+?\s+(?<name>\w+)\s*\{", RegexOptions.CultureInvariant)]
    private static partial Regex PublicProperty();

    // name="Input.CategoryLevelMax"  /  asp-for="CategoryLevelMax"  /  id='CategoryLevelMax'
    [GeneratedRegex(@"\b(?:name|id|asp-for)\s*=\s*[""'](?<name>[\w.\[\]]+)[""']", RegexOptions.CultureInvariant)]
    private static partial Regex MarkupField();

    [GeneratedRegex(@"^(?<key>[A-Za-z_][A-Za-z0-9_]*)=", RegexOptions.CultureInvariant)]
    private static partial Regex TemplateAssignment();
}
