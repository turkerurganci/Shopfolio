using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the data model rules that are written as bans (09 10.1 items 1, 2; 5.2 item 3; 9.4 item 5): an id is made only in
/// <c>EntityIds.cs</c> and never by the database, no name is given by hand, the three delete behaviors are the only ones, and the Turkish
/// letter constants live only in <c>TextNormalizer.cs</c>. The scan is text based and needs neither a database nor the network.
/// </summary>
internal static partial class DataModelSourceScan
{
    public const string IdGeneration = "makes an id outside EntityIds.cs";

    public const string DatabaseIdDefault = "gives the database the job of making an id";

    public const string HandWrittenName = "gives a table, column, index or constraint name by hand";

    public const string ForbiddenDeleteBehavior = "uses a delete behavior other than Cascade, SetNull and NoAction";

    public const string TurkishLetterConstant = "writes a Turkish letter constant outside TextNormalizer.cs";

    public const string NumericConversion = "converts a value to a number (an enum is stored as its code name; 09 10.1 item 3)";

    public const string ModuleTypeNameOutsideHasOne = "writes another module's type name as a plain string or uses its constant outside a HasOne call (09 10.1 item 1; D-61, D-62)";

    /// <summary>The file that may make ids.</summary>
    public const string EntityIdsFile = "EntityIds.cs";

    /// <summary>The file that may hold the Turkish letter constants.</summary>
    public const string TextNormalizerFile = "TextNormalizer.cs";

    /// <summary>
    /// Scans one file. The generated migration files (a <c>Migrations</c> folder, given by <paramref name="filePath"/>) are exempt from the
    /// hand-written name rule: the snapshot and the designer files spell out the names the convention derived (T103).
    /// </summary>
    public static IReadOnlyList<(string Rule, int Line)> Scan(string fileName, string text, string? filePath = null)
    {
        bool inMigrations = filePath is not null && filePath.Replace('\\', '/').Contains("/Migrations/", StringComparison.Ordinal);
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');

        // A module type name is written once, in a constant, and the constant is used only inside a HasOne call (09 10.1 item 1; D-61).
        HashSet<string> typeNameConstants = new(StringComparer.Ordinal);
        foreach (string line in lines)
        {
            Match declaration = ModuleTypeNameConstant().Match(line);
            if (declaration.Success)
            {
                typeNameConstants.Add(declaration.Groups[1].Value);
            }
        }

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            if (!inMigrations)
            {
                // A plain string is refused on a HasOne line too; only the constant's name is allowed there (D-62).
                bool inHasOne = line.Contains(".HasOne(", StringComparison.Ordinal);
                bool literalOutsideConstant = ModuleTypeNameLiteral().IsMatch(line) && !ModuleTypeNameConstant().IsMatch(line);
                bool constantOutsideHasOne = !inHasOne && typeNameConstants.Any(name => !ModuleTypeNameConstant().IsMatch(line) && Regex.IsMatch(line, $@"\b{Regex.Escape(name)}\b"));
                if (literalOutsideConstant || constantOutsideHasOne)
                {
                    findings.Add((ModuleTypeNameOutsideHasOne, i + 1));
                }
            }

            if (fileName != EntityIdsFile && GuidFactoryCall().IsMatch(line))
            {
                findings.Add((IdGeneration, i + 1));
            }

            if (DatabaseIdFunction().IsMatch(line))
            {
                findings.Add((DatabaseIdDefault, i + 1));
            }

            if (!inMigrations && HandWrittenNameCall().IsMatch(line))
            {
                findings.Add((HandWrittenName, i + 1));
            }

            if (ForbiddenDeleteBehaviorName().IsMatch(line))
            {
                findings.Add((ForbiddenDeleteBehavior, i + 1));
            }

            if (fileName != TextNormalizerFile && TurkishLetterLiteral().IsMatch(line))
            {
                findings.Add((TurkishLetterConstant, i + 1));
            }

            if (NumericConversionCall().IsMatch(line))
            {
                findings.Add((NumericConversion, i + 1));
            }
        }

        return findings;
    }

    [GeneratedRegex(@"\bGuid\s*\.\s*(NewGuid|CreateVersion7)\b")]
    private static partial Regex GuidFactoryCall();

    [GeneratedRegex(@"\b(gen_random_uuid|uuidv7|uuid_generate_v4)\s*\(", RegexOptions.IgnoreCase)]
    private static partial Regex DatabaseIdFunction();

    [GeneratedRegex(@"\.\s*(ToTable|HasColumnName|HasDatabaseName|HasConstraintName)\s*\(")]
    private static partial Regex HandWrittenNameCall();

    [GeneratedRegex(@"\bDeleteBehavior\s*\.\s*(Restrict|ClientCascade|ClientSetNull|ClientNoAction)\b")]
    private static partial Regex ForbiddenDeleteBehaviorName();

    [GeneratedRegex(@"\bHasConversion\s*<\s*(int|short|long|byte|sbyte|ushort|uint|ulong|Int16|Int32|Int64)\s*>|\bEnumToNumberConverter\b")]
    private static partial Regex NumericConversionCall();

    // A full name of a type in another module's own namespace; the Contracts namespace is the open surface and is not matched.
    [GeneratedRegex("\"Shopfolio\\.Modules\\.(?!\\w+\\.Contracts\\b)\\w+\\.[A-Z]\\w*\"")]
    private static partial Regex ModuleTypeNameLiteral();

    [GeneratedRegex("\\bconst\\s+string\\s+(\\w+)\\s*=\\s*\"Shopfolio\\.Modules\\.(?!\\w+\\.Contracts\\b)\\w+\\.[A-Z]\\w*\"")]
    private static partial Regex ModuleTypeNameConstant();

    [GeneratedRegex("'[ıİçÇğĞöÖşŞüÜ]'")]
    private static partial Regex TurkishLetterLiteral();
}
