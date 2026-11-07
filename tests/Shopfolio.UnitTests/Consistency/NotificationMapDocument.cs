using System.Text.RegularExpressions;

using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>A table row of 03 by its row id (the first cell) and its cells, in the document's order.</summary>
internal sealed record DocumentRow(string Id, IReadOnlyList<string> Cells);

/// <summary>
/// Reads the notification map out of 03 (T328; 03 7.1, 7.2, 7.3) and the notice cells of the flow tables (03 1, 2, 3, 4, 5, 6, 8, 9, 10): the document side of the
/// two-way comparison with <see cref="NotificationMap"/> (09 21.4 item 1). Reads <c>Docs/03_USER_FLOWS.md</c> only; nothing is written.
/// </summary>
internal static partial class NotificationMapDocument
{
    private const string Dot = "·";
    private const string Dash = "—";
    private const string EnDash = "–";

    /// <summary>The column of 7.1 that names the channel; every row starts with "E-posta" (03 7 "Kanal").</summary>
    public const string ChannelPrefix = "E-posta " + Dot + " ";

    private static readonly Lazy<string[]> DocumentLines = new(() =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "03_USER_FLOWS.md")).ReplaceLineEndings("\n").Split('\n'));

    /// <summary>The account and administration e-mails by the name 7.1 gives them in the "Bildirim" column.</summary>
    private static readonly Dictionary<string, DispatchType> AccountNames = new(StringComparer.Ordinal)
    {
        ["E-posta doğrulama bağlantısı"] = DispatchType.EmailVerificationLink,
        ["Şifre sıfırlama bağlantısı"] = DispatchType.PasswordResetLink,
        ["Yeni e-posta adresinin doğrulanması"] = DispatchType.NewEmailVerificationLink,
        ["E-posta değişikliğinin eski adrese bildirimi"] = DispatchType.EmailChangeNotice,
        ["Yönetici daveti"] = DispatchType.AdminInvitation,
        ["Hesabın silindiği bildirimi"] = DispatchType.AccountDeletedNotice,
        ["Google girişinin teyit bağlantısı"] = DispatchType.GoogleLinkConfirmationLink,
    };

    /// <summary>The "Kanal" column of 7.1 by its text; "eski adres" is told apart by the "Alıcı" column (the order's customer or the account's holder).</summary>
    private static readonly Dictionary<string, NoticeAddressee> Channels = new(StringComparer.Ordinal)
    {
        [ChannelPrefix + "siparişin iletişim e-postası"] = NoticeAddressee.OrderContact,
        [ChannelPrefix + "düzeltilmiş adres"] = NoticeAddressee.CorrectedOrderContact,
        [ChannelPrefix + "güncel adres"] = NoticeAddressee.CurrentOrderContact,
        [ChannelPrefix + "firma kimliğindeki iletişim e-postası"] = NoticeAddressee.FirmContact,
        [ChannelPrefix + "her yönetici hesabının kendi adresi"] = NoticeAddressee.EachAdministrator,
        [ChannelPrefix + "kaydın adresi"] = NoticeAddressee.RegisteredAddress,
        [ChannelPrefix + "hesabın adresi"] = NoticeAddressee.AccountAddress,
        [ChannelPrefix + "hesabın adresi " + Dash + " geri almada eski adres"] = NoticeAddressee.AccountAddressOrOldOnRevert,
        [ChannelPrefix + "yeni adres"] = NoticeAddressee.NewAddress,
        [ChannelPrefix + "davetlinin adresi"] = NoticeAddressee.InviteeAddress,
        [ChannelPrefix + "silinen hesabın adresi"] = NoticeAddressee.DeletedAccountAddress,
    };

    public static IReadOnlyList<DocumentRow> Triggers() => Rows("### 7.1 ", "### 7.2 ", TriggerRowId());

    public static IReadOnlyList<DocumentRow> NoNotices() => Rows("### 7.2 ", "### 7.3 ", NoNoticeRowId());

    public static IReadOnlyList<DocumentRow> Events() => Rows("### 7.3 ", "## 8. ", EventRowId());

    /// <summary>The notice a 7.1 row sends: null on the row that re-sends any of B-1…B-16 (7.1.37).</summary>
    public static DispatchType? TypeOf(DocumentRow row)
    {
        string cell = row.Cells[1];
        if (cell.Contains('…', StringComparison.Ordinal))
        {
            return null;
        }

        Match code = NoticeCode().Match(cell);
        if (code.Success && code.Index == 0)
        {
            return Enum.Parse<DispatchType>(code.Groups[1].Value + code.Groups[2].Value);
        }

        return AccountNames.TryGetValue(cell, out DispatchType type) ? type : throw new InvalidOperationException($"03 {row.Id} names a notice the test does not know: {cell}");
    }

    public static NoticeAddressee AddresseeOf(DocumentRow row)
    {
        string recipient = row.Cells[4];
        string channel = row.Cells[6];
        if (channel == ChannelPrefix + "eski adres")
        {
            return recipient.StartsWith("Müşteri", StringComparison.Ordinal) ? NoticeAddressee.OrderContactBeforeCorrection : NoticeAddressee.OldAccountAddress;
        }

        return Channels.TryGetValue(channel, out NoticeAddressee addressee) ? addressee : throw new InvalidOperationException($"03 {row.Id} names a channel the test does not know: {channel}");
    }

    /// <summary>The sources a row names, as "02 §9.2" and "K-712": a document number and section, a list of further sections after it, and decision numbers.</summary>
    public static IReadOnlyList<string> SourcesOf(string cell)
    {
        string text = cell.Replace("`", string.Empty, StringComparison.Ordinal);
        SortedSet<string> sources = new(StringComparer.Ordinal);
        foreach (Match match in SectionReference().Matches(text))
        {
            string document = match.Groups["doc"].Value;
            sources.Add($"{document} §{match.Groups["first"].Value}");
            foreach (Capture more in match.Groups["more"].Captures)
            {
                sources.Add($"{document} §{more.Value}");
            }
        }

        foreach (Match decision in DecisionNumber().Matches(text))
        {
            sources.Add(decision.Value);
        }

        return [.. sources];
    }

    /// <summary>The rows of 7.1 and 7.2 a 7.3 row names, a range ("7.1.16–7.1.18") expanded.</summary>
    public static IReadOnlyList<string> RowsNamedBy(string cell)
    {
        SortedSet<string> rows = new(RowOrder.Instance);
        foreach (Match range in RowRange().Matches(cell))
        {
            int first = int.Parse(range.Groups["a"].Value, System.Globalization.CultureInfo.InvariantCulture);
            int last = int.Parse(range.Groups["b"].Value, System.Globalization.CultureInfo.InvariantCulture);
            for (int n = first; n <= last; n++)
            {
                rows.Add($"{range.Groups["prefix"].Value}.{n}");
            }
        }

        foreach (Match single in RowId().Matches(RowRange().Replace(cell, " ")))
        {
            rows.Add(single.Value);
        }

        return [.. rows];
    }

    /// <summary>
    /// The notice cell of every flow row, by the row id: the "Bildirim" column of every table of 03 that has one, except the three tables of the map itself
    /// (7.1's "Bildirim" column names the notice, 7.3's the transitions' notices).
    /// </summary>
    public static IReadOnlyDictionary<string, string> FlowNoticeCells()
    {
        Dictionary<string, string> cells = new(StringComparer.Ordinal);
        string[]? header = null;
        foreach (string line in DocumentLines.Value)
        {
            if (!line.StartsWith('|'))
            {
                header = null;
                continue;
            }

            string[] parts = Cells(line);
            if (parts[0] == "#" && parts.Contains("Bildirim"))
            {
                header = parts;
                continue;
            }

            if (header is null || header.Contains("Geçiş ya da olay") || header.Any(h => h.StartsWith("Tetikleyici", StringComparison.Ordinal)) || parts.Length != header.Length
                || !FlowRowId().IsMatch(parts[0]))
            {
                continue;
            }

            cells[parts[0]] = parts[Array.IndexOf(header, "Bildirim")];
        }

        return cells;
    }

    /// <summary>Every row id that opens a table row anywhere in 03, flow tables or not (a row a 7.1 "Akış" cell names exists even where its table has no notice column).</summary>
    public static IReadOnlySet<string> AllRowIds()
    {
        HashSet<string> ids = new(StringComparer.Ordinal);
        foreach (string line in DocumentLines.Value.Where(l => l.StartsWith('|')))
        {
            string first = Cells(line)[0];
            if (FlowRowId().IsMatch(first))
            {
                ids.Add(first);
            }
        }

        return ids;
    }

    /// <summary>The flow rows a 7.1 "Akış" cell names ("§2.4.9, §2.10.1.5").</summary>
    public static IReadOnlyList<string> FlowRowsNamedBy(string cell) => [.. FlowReference().Matches(cell).Select(m => m.Groups[1].Value)];

    /// <summary>
    /// The notices a notice cell of a flow row makes: the B-n and F-n codes it names, except a code the cell says does NOT go ("B-7 gitmez", "B-16 gitmiştir" -
    /// the notice was sent before; 03 7.3 "kapanışta yok").
    /// </summary>
    public static IReadOnlyList<DispatchType> NoticesOf(string cell) =>
    [
        .. NoticeCode().Matches(cell)
            .Where(m => !NotMade().IsMatch(cell[(m.Index + m.Length)..]))
            .Select(m => Enum.Parse<DispatchType>(m.Groups[1].Value + m.Groups[2].Value))
            .Distinct(),
    ];

    private static IReadOnlyList<DocumentRow> Rows(string startPrefix, string endPrefix, Regex idPattern)
    {
        string[] lines = DocumentLines.Value;
        int start = Array.FindIndex(lines, l => l.StartsWith(startPrefix, StringComparison.Ordinal));
        Assert.True(start >= 0, $"03 no longer has the heading '{startPrefix}'.");
        int end = Array.FindIndex(lines, start + 1, l => l.StartsWith(endPrefix, StringComparison.Ordinal));
        Assert.True(end > start, $"03 no longer has the heading '{endPrefix}' after '{startPrefix}'.");
        List<DocumentRow> rows = [];
        foreach (string line in lines[start..end].Where(l => l.StartsWith('|')))
        {
            string[] cells = Cells(line);
            if (idPattern.IsMatch(cells[0]))
            {
                rows.Add(new DocumentRow(cells[0], cells));
            }
        }

        return rows;
    }

    private static string[] Cells(string line) => [.. line.Trim().Trim('|').Split(" | ").Select(c => c.Trim())];

    /// <summary>Orders row ids as numbers ("7.1.9" before "7.1.10").</summary>
    private sealed class RowOrder : IComparer<string>
    {
        public static readonly RowOrder Instance = new();

        public int Compare(string? x, string? y)
        {
            int[] a = [.. x!.Split('.').Select(int.Parse)];
            int[] b = [.. y!.Split('.').Select(int.Parse)];
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                int c = a[i].CompareTo(b[i]);
                if (c != 0)
                {
                    return c;
                }
            }

            return a.Length.CompareTo(b.Length);
        }
    }

    [GeneratedRegex(@"^7\.1\.\d+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TriggerRowId();

    [GeneratedRegex(@"^7\.2\.\d+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NoNoticeRowId();

    [GeneratedRegex(@"^7\.3\.\d+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex EventRowId();

    [GeneratedRegex(@"^\d+(\.\d+)+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FlowRowId();

    [GeneratedRegex(@"\b([BF])-(\d+)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NoticeCode();

    [GeneratedRegex(@"^\s+(gitmez|gitmiştir)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NotMade();

    [GeneratedRegex(@"\b(?<doc>\d\d) §(?<first>\d+(?:\.\d+)*)(?:, §(?<more>\d+(?:\.\d+)*))*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SectionReference();

    [GeneratedRegex(@"\bK-\d+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DecisionNumber();

    [GeneratedRegex(@"(?<prefix>7\.[12])\.(?<a>\d+)" + EnDash + @"(?:7\.[12]\.)?(?<b>\d+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RowRange();

    [GeneratedRegex(@"7\.[12]\.\d+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RowId();

    [GeneratedRegex(@"§(\d+(?:\.\d+)+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FlowReference();
}
