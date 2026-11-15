using System.Reflection;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The code side of a field row's validation (T547 criterion 6, both directions; 04 7 introduction; 09 21.4 item 1): which kinds of check the code makes for the field - the validation attributes of its
/// property (read by reflection) and the enforcement lines of <see cref="FormFieldEnforcementTable"/> (a handler, a module or a view that checks a field that carries no attribute). The comparison with the row:
/// <c>RequirementWithoutEnforcement</c> - the row says the field is required and the code has no required check; <c>RequirementContradictsCode</c> - the code requires it and the row says it is not required;
/// <c>RuleWithoutEnforcement</c> - the row names a rule, a parameter or a limit and the code has no check of its kind; <c>RuleMissesAttribute</c> - the code has a check whose rule the row does not name;
/// <c>RuleKindUnmapped</c> - the row names an id, or the code has a kind of check, that the mapping table does not hold (nothing is skipped silently); <c>EnforcementNotInCode</c> - an enforcement line of the table
/// is not in the file or does not hold the token of its kind.
/// </summary>
internal static class FormFieldEnforcement
{
    /// <summary>The kinds of check and the tokens a code line of that kind holds (an error code of <c>FieldCodes</c> or <c>ErrorCode</c>, an attribute, a symbol of the parameter or the limit).</summary>
    public static readonly IReadOnlyDictionary<string, string[]> KindTokens = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["Required"] = ["FieldRequired", "IsNullOrWhiteSpace", "PasswordPolicy", "RequiredText", "Require(", "CheckAddress", "PublishCondition", "ResolveOrderAddresses", "CorrectionValueProblem.Missing", "ContentGateField", "TradesmanFields", "MerchantFields", "RequiredWithCeiling", "NotSearched", "No word", "ConfirmationRequired", "startRequired: true", "ItemNotSelected", "WarningsConfirmed", "DeliveryOrderMissing", "AsksDeliveryOrder", "TryReadQuantity", "CheckFenced", "RequiredName", "FilledFieldCleared", "true, errors", "ShipmentInfoMissing", "DeliverySequenceRequired", "NoItemText"],
        ["CheckboxAccepted"] = ["CheckboxNotAccepted"],
        ["Email"] = ["InvalidEmail"],
        ["Password"] = ["PasswordTooWeak", "PasswordPolicy"],
        ["Phone"] = ["InvalidPhone", "TryStorePhone", "ResolveOrderAddresses"],
        ["Province"] = ["InvalidProvince"],
        ["List"] = ["InvalidListValue"],
        ["Iban"] = ["InvalidIban", "NoticeIbanRule"],
        ["Amount"] = ["InvalidAmount", "ReadPrice"],
        ["Percentage"] = ["InvalidPercentage"],
        ["Quantity"] = ["InvalidQuantity", "TryReadUnits", "TryReadQuantity"],
        ["Date"] = ["InvalidDate", "ReadDates", "TryReadDate", "CheckRange"],
        ["DateRange"] = ["InvalidDateRange", "CheckRange"],
        ["Url"] = ["InvalidUrl"],
        ["TextCap"] = ["TextTooLong", "Ceiling"],
        ["Image"] = ["InvalidImage"],
        ["Color"] = ["InvalidColor"],
        ["Period"] = ["PeriodFences", "ParameterFences", "FenceBounds", "FirmSettingFences", "CheckShippingPeriod", "CheckFenced"],
        ["SearchTrim"] = ["ProductNameMaxLength"],
        ["CouponCode"] = ["NormalizedCode"],
        ["NetQuantity"] = ["InvalidNetQuantity"],
    };

    /// <summary>The kinds of check that stand for the rule of a field type of <c>04 7.3.2</c>: the code has at least one of them for a row that names the rule.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> RuleKinds = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["7.3.2.1"] = ["Email"],
        ["7.3.2.2"] = ["Password"],
        ["7.3.2.3"] = ["Required"],
        ["7.3.2.4"] = ["Phone"],
        ["7.3.2.5"] = ["Required", "Phone", "Province"],
        ["7.3.2.6"] = ["Province", "List"],
        ["7.3.2.7"] = ["Iban"],
        ["7.3.2.8"] = ["Amount"],
        ["7.3.2.9"] = ["Percentage"],
        ["7.3.2.10"] = ["Quantity"],
        ["7.3.2.11"] = ["Date", "Period", "Quantity"],
        ["7.3.2.12"] = ["Date", "DateRange"],
        ["7.3.2.13"] = ["Url"],
        ["7.3.2.14"] = ["TextCap"],
        ["7.3.2.15"] = ["Required"],
        ["7.3.2.16"] = ["List"],
        ["7.3.2.17"] = ["SearchTrim"],
        ["7.3.2.18"] = ["Image"],
        ["7.3.2.19"] = ["CouponCode"],
        ["8.2.3"] = ["NetQuantity"],
    };

    /// <summary>The ids of <c>02 11</c> (parameters P-, periods Z-) and of the attempt limits L- and the symbols of the code that read them; a row that names one needs an enforcement line holding the symbol.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> IdTokens = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["P-5"] = ["\"P-5\""],
        ["P-6"] = ["\"P-6\""],
        ["P-8"] = ["\"P-8\""],
        ["P-9"] = ["\"P-9\"", "MaxQuantityPerOrderExceeded"],
        ["P-10"] = ["DefaultVatRate"],
        ["P-21"] = ["OptionDimensionMax"],
        ["P-20"] = ["CategoryLevelMax"],
        ["P-22"] = ["ImageFormats"],
        ["P-23"] = ["ImageMaxBytes"],
        ["P-24"] = ["ImagesPerProductMax", "ImageLimitExceeded"],
        ["P-25"] = ["ImagesPerContentRecordMax", "ImageLimitExceeded"],
        ["P-26"] = ["DigitalFileMaxBytes"],
        ["P-27"] = ["ProductNameMaxLength", "TitleMaxLength"],
        ["P-28"] = ["ShortTextMaxLength", "ShortDescriptionMaxLength"],
        ["P-29"] = ["ShortIntroductionMaxLength"],
        ["P-30"] = ["AnnouncementTextMaxLength"],
        ["P-44"] = ["\"P-44\""],
        ["Z-4"] = ["ShortSessionHours"],
        ["Z-5"] = ["LongSessionDays"],
        ["L-1"] = ["AttemptLimit.L1"],
        ["L-2"] = ["AttemptLimit.L2"],
        ["L-3"] = ["AttemptLimit.L3"],
        ["L-4"] = ["AttemptLimit.L4"],
        ["L-5"] = ["AttemptLimit.L5"],
        ["L-6"] = ["AttemptLimit.L6"],
        ["L-7"] = ["AttemptLimit.L7"],
        ["L-8"] = ["AttemptLimit.L8"],
        ["L-9"] = ["AttemptLimit.L9"],
    };

    /// <summary>The validation attributes of the code and the kind of check each is. Every attribute of the rule family must be here (<c>RuleKindUnmapped</c> otherwise).</summary>
    public static readonly IReadOnlyDictionary<string, string> AttributeKinds = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["FieldRequiredAttribute"] = "Required",
        ["InvalidEmailAttribute"] = "Email",
        ["PasswordTooWeakAttribute"] = "Password",
        ["InvalidPhoneAttribute"] = "Phone",
        ["InvalidIbanAttribute"] = "Iban",
        ["InvalidAmountAttribute"] = "Amount",
        ["InvalidNetQuantityAttribute"] = "NetQuantity",
        ["InvalidPercentageAttribute"] = "Percentage",
        ["InvalidQuantityAttribute"] = "Quantity",
        ["InvalidDateAttribute"] = "Date",
        ["InvalidUrlAttribute"] = "Url",
        ["TextTooLongAttribute"] = "TextCap",
        ["InvalidListValueAttribute"] = "List",
        ["InvalidColorAttribute"] = "Color",
        ["CheckboxNotAcceptedAttribute"] = "CheckboxAccepted",
    };

    private static readonly string[] RequiredWords = ["Zorunlu", "Yayın için zorunlu", "Meslek odası"];

    /// <summary>
    /// The problems of one row: <paramref name="attributeMarks"/> are the attributes of the row's property places (type names), <paramref name="enforcement"/> the table's lines of the row
    /// (<c>Kind|file#fragment</c>), <paramref name="ids"/> the rule ids of the rule and requirement cells and <paramref name="limits"/> the limit ids of the limit cell.
    /// </summary>
    public static IEnumerable<string> Problems(
        string row,
        string requirement,
        IReadOnlyList<string> attributeMarks,
        IReadOnlyList<string> enforcement,
        IReadOnlyCollection<string> ids,
        IReadOnlyCollection<string> limits,
        IReadOnlySet<string> knownRows,
        IReadOnlyDictionary<string, string> sources)
    {
        List<string> problems = [];
        HashSet<string> kinds = new(StringComparer.Ordinal);

        foreach (string mark in attributeMarks)
        {
            if (AttributeKinds.TryGetValue(mark, out string? kind))
            {
                kinds.Add(kind);
            }
            else
            {
                problems.Add($"RuleKindUnmapped:{row}: the property carries {mark}, a rule attribute the kind table does not hold.");
            }
        }

        foreach (string line in enforcement)
        {
            int bar = line.IndexOf('|', StringComparison.Ordinal);
            int hash = line.IndexOf('#', StringComparison.Ordinal);
            if (bar <= 0 || hash < bar)
            {
                problems.Add($"EnforcementNotInCode:{row}: '{line}' is not of the form Kind|file#fragment.");
                continue;
            }

            string kind = line[..bar];
            string file = line[(bar + 1)..hash];
            string fragment = line[(hash + 1)..];
            string[]? tokens = KindTokens.TryGetValue(kind, out string[]? known) ? known : IdTokens.GetValueOrDefault(kind);
            if (tokens is null)
            {
                problems.Add($"RuleKindUnmapped:{row}: the enforcement line '{line}' has a kind the mapping tables do not hold.");
                continue;
            }

            if (!sources.TryGetValue(file, out string? text) || !Together(text, fragment))
            {
                problems.Add($"EnforcementNotInCode:{row}: '{fragment}' is not in {file}.");
                continue;
            }

            if (!tokens.Any(t => fragment[(fragment.IndexOf(Pair, StringComparison.Ordinal) is var cut and >= 0 ? cut : 0)..].Contains(t, StringComparison.Ordinal)))
            {
                problems.Add($"EnforcementNotInCode:{row}: '{fragment}' does not hold a token of the kind {kind} ({string.Join(", ", tokens)}).");
                continue;
            }

            kinds.Add(kind);
        }

        bool required = RequiredWords.Any(w => requirement.StartsWith(w, StringComparison.Ordinal));
        bool requiredInCode = kinds.Contains("Required") || kinds.Contains("CheckboxAccepted") || (kinds.Contains("Quantity") && ids.Contains("7.3.2.10"));
        if (required && !requiredInCode)
        {
            problems.Add($"RequirementWithoutEnforcement:{row}: the row says '{requirement}' and the code has no required check for the field.");
        }

        if (!required && kinds.Contains("Required"))
        {
            problems.Add($"RequirementContradictsCode:{row}: the code requires the field, the row says '{requirement}'.");
        }

        HashSet<string> justified = new(StringComparer.Ordinal);
        foreach (string id in ids.Concat(limits))
        {
            if (RuleKinds.TryGetValue(id, out string[]? anyOf))
            {
                justified.UnionWith(anyOf);
                if (!anyOf.Any(kinds.Contains))
                {
                    problems.Add($"RuleWithoutEnforcement:{row}: the row names {id} and the code has none of the checks [{string.Join(", ", anyOf)}] for the field.");
                }
            }
            else if (IdTokens.ContainsKey(id))
            {
                justified.Add(id);
                if (!kinds.Contains(id))
                {
                    problems.Add($"RuleWithoutEnforcement:{row}: the row names {id} and no enforcement line of the field reads it.");
                }
            }
            else if (id.StartsWith("7.3.2.", StringComparison.Ordinal) || id.StartsWith("P-", StringComparison.Ordinal) || id.StartsWith("Z-", StringComparison.Ordinal) || id.StartsWith("L-", StringComparison.Ordinal))
            {
                problems.Add($"RuleKindUnmapped:{row}: the row names {id}, which the mapping tables do not hold.");
            }
            else if (!knownRows.Contains(id))
            {
                problems.Add($"RuleKindUnmapped:{row}: the row points to {id}, which is not a row or a block of the document.");
            }
        }

        foreach (string kind in kinds.Where(k => k is not "Required" and not "CheckboxAccepted"))
        {
            if (!justified.Contains(kind))
            {
                problems.Add($"RuleMissesAttribute:{row}: the code has a {kind} check for the field, the row names no rule of that kind.");
            }
        }

        return problems;
    }

    /// <summary>The separator of the two parts of an enforcement fragment: the part that names the field and the part that holds the kind's token, found in the same file within <see cref="Within"/> lines.</summary>
    public const string Pair = "@@";

    /// <summary>The separator of two pieces that need only be in the same file (a field named in a table of required fields and the code that enforces the table).</summary>
    public const string Anywhere = "@@@";

    private const int Within = 12;

    /// <summary>A fragment is one piece of a line, or two pieces <c>field@@check</c> that stand within twelve lines of each other (each held as a whole identifier at its ends that are identifier characters).</summary>
    private static bool Together(string text, string fragment)
    {
        int far = fragment.IndexOf(Anywhere, StringComparison.Ordinal);
        if (far >= 0)
        {
            return FormFieldCheck.Holds(text, fragment[..far]) && FormFieldCheck.Holds(text, fragment[(far + Anywhere.Length)..]);
        }

        int cut = fragment.IndexOf(Pair, StringComparison.Ordinal);
        if (cut < 0)
        {
            return FormFieldCheck.Holds(text, fragment);
        }

        string field = fragment[..cut];
        string check = fragment[(cut + Pair.Length)..];
        string[] lines = text.Split('\n');
        int[] fieldLines = [.. Enumerable.Range(0, lines.Length).Where(i => FormFieldCheck.Holds(lines[i], field))];
        int[] checkLines = [.. Enumerable.Range(0, lines.Length).Where(i => FormFieldCheck.Holds(lines[i], check))];
        return fieldLines.Any(f => checkLines.Any(c => Math.Abs(c - f) <= Within));
    }

    /// <summary>The attribute type names of a property that belong to the rule family, and a rule attribute the table does not hold is returned too (a subclass of <c>FieldRuleAttribute</c>).</summary>
    public static IEnumerable<string> RuleMarks(PropertyInfo property)
    {
        return property.GetCustomAttributes().Where(a => a is FieldRuleAttribute).Select(a => a.GetType().Name);
    }
}
