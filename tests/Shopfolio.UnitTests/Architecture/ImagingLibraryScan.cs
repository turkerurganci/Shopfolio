using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the image library's namespaces (T223 KK1; 09 15.6 item 1; 09 6 item 4; 09 19.2 item 3). Two rules over the application
/// source: (1) the library's namespace (<c>SkiaSharp</c>) is named only in <c>Infrastructure/Imaging/</c> - a library change then changes only
/// that folder; (2) the namespaces of the two candidates that were weighed and not taken (<c>SixLabors</c>, <c>ImageMagick</c>; K-1575) are
/// named nowhere. Full-line comments are blanked first and keep their line (a comment may say the library's name). The test projects are not
/// scanned: the image tests read an image back with the library, which is their purpose. The scan reads text and writes nothing.
/// </summary>
internal static partial class ImagingLibraryScan
{
    public const string LibraryOutsideImaging = "names the image library's namespace outside Infrastructure/Imaging (09 15.6 item 1: the library is touched in one folder)";
    public const string RejectedLibrary = "names a rejected image library's namespace (K-1575: SixLabors and ImageMagick were weighed and not taken)";

    /// <summary>The one folder that may name the library.</summary>
    public const string ImagingFolder = "src/Shopfolio.Web/Infrastructure/Imaging/";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string code = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        if (!path.StartsWith(ImagingFolder, StringComparison.Ordinal))
        {
            Add(findings, LibraryOutsideImaging, Library(), code);
        }

        Add(findings, RejectedLibrary, Rejected(), code);
        return [.. findings.OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            int line = text.AsSpan(0, match.Index).Count('\n') + 1;
            if (!findings.Contains((rule, line)))
            {
                findings.Add((rule, line));
            }
        }
    }

    private static string BlankCommentLines(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    [GeneratedRegex(@"\bSkiaSharp\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Library();

    [GeneratedRegex(@"\b(?:SixLabors|ImageMagick|Magick)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Rejected();
}
