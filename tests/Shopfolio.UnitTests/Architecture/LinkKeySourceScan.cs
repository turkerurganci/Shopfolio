using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the key and link rules (T222; 09 15.1 items 1, 3 and 4; 09 19.2 item 3): (1) the cryptographic generator and the digest
/// functions are named only in <c>Infrastructure/Security/</c> and <c>Modules/Membership/</c> - a closed list of other files may be added with
/// its reason; (2) inside those two folders no weaker source makes a key (<c>Random</c>, <c>Guid.NewGuid</c>, <c>Guid.CreateVersion7</c>);
/// (3) a link key is derived only where an e-mail body is made (<c>Modules/Notifications/</c>) and inside <c>LinkKeys.cs</c>; (4) a link is consumed
/// only from an <c>OnPost…</c> handler (a link's GET changes nothing, MK-51), the one dispensation being <see cref="ConsumingMethods"/> (a module's consuming command, whose callers are held to the same rule). The scan reads the whole text, so a call split over several lines
/// is found; full-line comments are blanked first and keep their line. The test projects are not scanned.
/// </summary>
internal static partial class LinkKeySourceScan
{
    public const string GeneratorOutsideSecurity = "names RandomNumberGenerator, SHA256 or HMACSHA256 outside Infrastructure/Security and Modules/Membership (09 15.1 item 1)";
    public const string WeakKeySource = "makes a key or a mark from Random, Guid.NewGuid or Guid.CreateVersion7 (09 15.1 item 1: only the cryptographic generator)";
    public const string DeriveOutsideNotifications = "derives a link key outside Modules/Notifications and LinkKeys.cs (09 15.1 item 4: the key is made where the e-mail body is made)";
    public const string ConsumeOutsidePost = "consumes a link outside an OnPost handler (09 15.1 items 3 and 4; 05 6.4.7: opening a link changes no record)";

    /// <summary>The folders that may name the generator and the digest functions.</summary>
    public static readonly string[] GeneratorFolders = ["src/Shopfolio.Web/Infrastructure/Security/", "src/Shopfolio.Web/Modules/Membership/"];

    /// <summary>
    /// The closed list of other files that may name the generator or a digest function, each with its reason (09 15.1 item 1: the order number,
    /// the content digests of 15.6 item 5, 11.4 item 5 and 15.7 item 2). A file is added by the task that owns it, one path, with its reason:
    /// <c>Modules/Catalog/ImageUploads.cs</c> - the content digest that names the source copy (09 15.6 item 5; T223);
    /// <c>Modules/Settings/LegalTextTemplates.cs</c> - the content digest that is the version id of a legal text template (09 11.4 item 5; T366);
    /// <c>Infrastructure/Integrations/GoogleAuthorizationRequest.cs</c> - the S256 proof key challenge, the digest of the PKCE verifier that the authorization request carries (08 2.3; 09 11.3; T409).
    /// </summary>
    public static readonly string[] GeneratorExceptions =
    [
        "src/Shopfolio.Web/Modules/Catalog/ImageUploads.cs",
        "src/Shopfolio.Web/Modules/Settings/LegalTextTemplates.cs",
        "src/Shopfolio.Web/Infrastructure/Integrations/GoogleAuthorizationRequest.cs",
    ];

    /// <summary>The folder where a link key may be derived.</summary>
    public const string DeriveFolder = "src/Shopfolio.Web/Modules/Notifications/";

    /// <summary>The file that holds the derivation itself (and the digest function that calls it).</summary>
    public const string LinkKeysFile = "src/Shopfolio.Web/Infrastructure/Security/LinkKeys.cs";

    /// <summary>The file that holds the consuming itself.</summary>
    public const string LinkRecordsFile = "src/Shopfolio.Web/Infrastructure/Security/LinkRecords.cs";

    /// <summary>
    /// The closed list of files that consume a link outside a page handler, each with the one method that may (T341): the Membership module's verification command is
    /// the page's POST work, joined to the caller's transaction; it consumes the link in <c>VerifyEmailAsync</c> and nowhere else in the file, and a call of
    /// <c>VerifyEmailAsync</c> is itself held to the <c>OnPost</c> rule, so the link's GET still changes nothing (MK-51).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string[]> ConsumingMethods = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["src/Shopfolio.Web/Modules/Membership/MembershipModule.cs"] = ["VerifyEmailAsync"],

        // The Google link confirmation and the new e-mail verification (T343): each consumes its link in its one command and nowhere else in the file.
        ["src/Shopfolio.Web/Modules/Membership/GoogleSignIns.cs"] = ["ConfirmGoogleLinkAsync"],
        ["src/Shopfolio.Web/Modules/Membership/EmailChanges.cs"] = ["VerifyNewEmailAsync"],

        // The one password rule's reset button (T342): both account types consume the password reset link here and nowhere else.
        ["src/Shopfolio.Web/Infrastructure/Security/PasswordRegime.cs"] = ["ResetPasswordAsync"],

        // The administrator invitation's button and the administrator's new e-mail verification (T379): each consumes its link in its one command and nowhere else in the file.
        ["src/Shopfolio.Web/Modules/Administration/AdminAccounts.cs"] = ["AcceptInvitationAsync"],
        ["src/Shopfolio.Web/Modules/Administration/AdminEmailChanges.cs"] = ["VerifyNewEmailAsync"],
    };

    /// <summary>
    /// The closed list of the modules' thin wrappers of a consuming command (T342): the method of the same name that only passes the call on to the consuming code. The wrapper's
    /// own name is the called name, so the dispensation covers nothing else in the file, and its callers are held to the <c>OnPost</c> rule like any caller of the command.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string[]> WrapperMethods = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["src/Shopfolio.Web/Modules/Membership/MembershipModule.cs"] = ["ResetPasswordAsync", "ConfirmGoogleLinkAsync", "VerifyNewEmailAsync"],
        ["src/Shopfolio.Web/Modules/Administration/AdministrationModule.cs"] = ["ResetPasswordAsync", "AcceptInvitationAsync", "VerifyNewEmailAsync"],
    };

    private static readonly string[] ControlKeywords = ["if", "for", "foreach", "while", "switch", "using", "lock", "catch", "fixed", "when", "return", "nameof", "typeof"];

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];

        bool inGeneratorFolder = GeneratorFolders.Any(f => path.StartsWith(f, StringComparison.Ordinal));
        if (!inGeneratorFolder && !GeneratorExceptions.Contains(path, StringComparer.Ordinal))
        {
            Add(findings, GeneratorOutsideSecurity, GeneratorNames(), clean);
        }

        if (inGeneratorFolder)
        {
            Add(findings, WeakKeySource, WeakSources(), clean);
        }

        if (!path.StartsWith(DeriveFolder, StringComparison.Ordinal) && !string.Equals(path, LinkKeysFile, StringComparison.Ordinal))
        {
            Add(findings, DeriveOutsideNotifications, DeriveCall(), clean);
        }

        if (!string.Equals(path, LinkRecordsFile, StringComparison.Ordinal))
        {
            FindConsumeOutsidePost(findings, path, clean);
        }

        return [.. findings.OrderBy(f => f.Line)];
    }

    private static void FindConsumeOutsidePost(List<(string Rule, int Line)> findings, string path, string text)
    {
        List<(string Name, int Start, int End)> methods = Methods(text);
        ConsumingMethods.TryGetValue(path, out string[]? ownMethods);
        WrapperMethods.TryGetValue(path, out string[]? wrappers);
        foreach (Match call in ConsumeCall().Matches(text).Concat(VerifyCall().Matches(text)))
        {
            // The outermost method that holds the call: a local function inside OnGet is still part of the handler.
            (string Name, int Start, int End)? owner = methods
                .Where(m => m.Start <= call.Index && call.Index < m.End)
                .OrderByDescending(m => m.End - m.Start)
                .Cast<(string Name, int Start, int End)?>()
                .FirstOrDefault();
            bool isPost = owner is not null && owner.Value.Name.StartsWith("OnPost", StringComparison.Ordinal);

            // The one dispensation: the consuming command of a module, in its one named method; its callers are held to the OnPost rule by <see cref="VerifyCall"/>.
            bool isOwnMethod = ownMethods is not null && owner is not null && ownMethods.Contains(owner.Value.Name, StringComparer.Ordinal)
                && call.Value.StartsWith("TryConsumeAsync", StringComparison.Ordinal);

            // The wrapper: a listed method that makes the call of its own name (ResetPasswordAsync calling ResetPasswordAsync) and nothing else.
            bool isWrapper = wrappers is not null && owner is not null && wrappers.Contains(owner.Value.Name, StringComparer.Ordinal)
                && Regex.Replace(call.Value, @"[\s.(]", string.Empty) == owner.Value.Name;
            if (!isPost && !isOwnMethod && !isWrapper)
            {
                Record(findings, ConsumeOutsidePost, text, call.Index);
            }
        }
    }

    // The bodies of the methods: a name, a parenthesis without nested parentheses, then a block or an expression body. Control statements and
    // type declarations are not methods.
    private static List<(string Name, int Start, int End)> Methods(string text)
    {
        List<(string Name, int Start, int End)> methods = [];
        foreach (Match header in MethodHeader().Matches(text))
        {
            string name = header.Groups["name"].Value;
            if (ControlKeywords.Contains(name, StringComparer.Ordinal))
            {
                continue;
            }

            int end = header.Groups["arrow"].Success
                ? text.IndexOf(';', header.Index + header.Length)
                : EndOfBlock(text, header.Index + header.Length - 1);
            if (end > header.Index)
            {
                methods.Add((name, header.Index, end));
            }
        }

        return methods;
    }

    private static int EndOfBlock(string text, int openBrace)
    {
        int depth = 0;
        for (int i = openBrace; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                depth++;
            }
            else if (text[i] == '}' && --depth == 0)
            {
                return i + 1;
            }
        }

        return text.Length;
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            Record(findings, rule, text, match.Index);
        }
    }

    private static void Record(List<(string Rule, int Line)> findings, string rule, string text, int index)
    {
        int line = text.AsSpan(0, index).Count('\n') + 1;
        if (!findings.Contains((rule, line)))
        {
            findings.Add((rule, line));
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

    [GeneratedRegex(@"\b(?:RandomNumberGenerator|SHA256|HMACSHA256)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex GeneratorNames();

    [GeneratedRegex(@"\bnew\s+Random\s*\(|\bRandom\s*\.\s*Shared\b|\bGuid\s*\.\s*(?:NewGuid|CreateVersion7)\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WeakSources();

    // Any call of a member named Derive: in the application source it can only be the link key derivation.
    [GeneratedRegex(@"\.\s*Derive\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DeriveCall();

    [GeneratedRegex(@"\bTryConsumeAsync\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ConsumeCall();

    // A call of the module command that consumes the verification link (a member access, so the declaration is not a call): only an OnPost handler may make it.
    [GeneratedRegex(@"\.\s*(?:VerifyEmailAsync|ResetPasswordAsync|ConfirmGoogleLinkAsync|VerifyNewEmailAsync)\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex VerifyCall();

    [GeneratedRegex(@"(?<!\b(?:class|record|struct|interface|new)\s+)\b(?<name>\w+)\s*\([^()]*\)\s*(?:where\s+[^{=;]*)?(?:(?<arrow>=>)|\{)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MethodHeader();
}
