using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The comparison of the state by role matrix of <c>04 6.1</c> to <c>6.3</c> with the code (T547; 09 19.2 item 3, 21.4 item 1). A problem is a line that says what differs: <c>ScreenOnlyIn…</c> and
/// <c>BlockMissing</c>, <c>BlockWithoutScreen</c> - a screen without a block or a block without a screen; <c>PageMissing</c>, <c>PageWithoutScreen</c> - a screen whose page is not in the code or a page that
/// no screen carries; <c>GateRowsDiffer</c>, <c>GateReadDiffers</c> - the screens a gate changes in the sentence of 6.1.5.2, in the rows and in the code; <c>AdministratorRow…</c>, <c>DraftRead…</c> - the same for the
/// administrator of 6.1.5.1; <c>StatusName…</c> - a status the rows name that is not the name of <c>02 5</c>; <c>OperationNotInMatrix</c> - an operation of <c>03 1.11</c> that no row's source names;
/// <c>PanelNotSingleRole</c>, <c>PanelPageOutsidePanel</c>, <c>PanelFormOutsidePanel</c> - the panel's one role and one address root (<c>02 10.1</c>; 04 6.1.3). There is no list of known differences.
/// </summary>
internal static partial class StateRoleCheck
{
    private static readonly string[] SameWords = ["aynı", "etkilenmez", "değişmez"];

    /// <summary>Words that name a status other than the one <c>02 5</c> names it (the synonyms 04 6.1.8 forbids), matched as whole words.</summary>
    private static readonly string[] Synonyms =
    [
        "Yayınlandı", "Yayında değil", "Aktif", "Pasif", "Arşivlendi", "Arşivde", "Gönderildi", "Kargolandı", "Teslim alındı", "Ödenmedi",
        "Beklemede", "İade edildi", "Kısmi iade", "Çözümlendi", "Kapandı", "Kapalı talep", "Tamamlandı", "Hazırlandı", "Onaylandı",
    ];

    /// <summary>
    /// D-1778: the panel screens that 04 6.1.5.2 names as showing the sales gate and whose page does not read it today (the payment methods and the shipping and defaults pages);
    /// closing the gap needs product code (T604 U), so the reverse direction of the gate check lists them until then.
    /// </summary>
    public static readonly IReadOnlyList<string> PanelGateReadGaps = ["E-45", "E-46"];

    public static IReadOnlyList<string> Problems(string document, IReadOnlyDictionary<string, string> sources, string requirements)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(requirements);

        List<string> problems = [];
        IReadOnlyList<ScreenListing> listed = StateRoleDocument.Screens(document);
        IReadOnlyList<ScreenBlock> blocks = StateRoleDocument.Blocks(document);
        Dictionary<string, string> routes = Routes(sources);

        problems.AddRange(Blocks(document, listed, blocks));
        problems.AddRange(Pages(sources, routes));
        problems.AddRange(Gates(document, sources, routes, blocks));
        problems.AddRange(Administrator(document, sources, routes, blocks));
        problems.AddRange(StatusNames(blocks, requirements));
        problems.AddRange(Operations(document, blocks));
        problems.AddRange(Panel(sources, routes, blocks));
        return problems;
    }

    /// <summary>The page routes of the web project by file: the text after <c>@page</c> of every view that starts with it.</summary>
    public static Dictionary<string, string> Routes(IReadOnlyDictionary<string, string> sources)
    {
        Dictionary<string, string> routes = new(StringComparer.Ordinal);
        foreach ((string file, string text) in sources.Where(s => s.Key.StartsWith("Pages/", StringComparison.Ordinal) && s.Key.EndsWith(".cshtml", StringComparison.Ordinal)))
        {
            Match match = PageDirective().Match(text);
            if (match.Success)
            {
                routes[file] = match.Groups[1].Value;
            }
        }

        return routes;
    }

    // 04 6.1.1: every screen is a block, 6.2.n = E-n and 6.3.k by the inventory; the block's rows are numbered without a gap and every cell says something.
    private static IEnumerable<string> Blocks(string document, IReadOnlyList<ScreenListing> listed, IReadOnlyList<ScreenBlock> blocks)
    {
        IReadOnlyList<ScreenEntry> entries = StateRoleInventory.Screens;
        foreach (string line in Relabel("04 4 screens and the code", listed.Select(l => l.Screen), entries.Select(e => e.Screen), "ScreenOnlyInDocument", "ScreenOnlyInCode"))
        {
            yield return line;
        }

        foreach (string line in Relabel("04 6 blocks and 04 4 screens", blocks.Select(b => b.Screen), listed.Select(l => l.Screen), "BlockWithoutScreen", "BlockMissing"))
        {
            yield return line;
        }

        Match stated = ScreenCount().Match(string.Join('\n', DataModelDocument.Section(document, "### 6.1 ").Split('\n').Where(l => l.StartsWith("**6.1.8", StringComparison.Ordinal))));
        if (!stated.Success || int.Parse(stated.Groups[1].Value, CultureInfo.InvariantCulture) != blocks.Count)
        {
            yield return $"BlockCountDiffers: 04 6.1.8 says '{(stated.Success ? stated.Groups[1].Value : "no count")}' screens carry a block, the document has {blocks.Count} blocks.";
        }

        foreach (ScreenBlock block in blocks)
        {
            ScreenEntry? entry = entries.FirstOrDefault(e => e.Screen == block.Screen);
            if (entry is not null && entry.Block != block.Block)
            {
                yield return $"BlockIdDiffers:{block.Screen}: the document's block is {block.Block}, the inventory says {entry.Block}.";
            }

            ScreenListing? screen = listed.FirstOrDefault(l => l.Screen == block.Screen);
            if (screen is not null && !string.Equals(screen.Name, block.Title, StringComparison.Ordinal))
            {
                yield return $"TitleDiffers:{block.Screen}: the block is titled '{block.Title}', 04 4 names the screen '{screen.Name}'.";
            }

            if (block.Rows.Count == 0)
            {
                yield return $"BlockEmpty:{block.Block}: a block holds at least one row.";
            }

            for (int i = 0; i < block.Rows.Count; i++)
            {
                MatrixRow row = block.Rows[i];
                if (row.Id != $"{block.Block}.{i + 1}")
                {
                    yield return $"RowIdDiffers:{block.Block}: the row at place {i + 1} is {row.Id}.";
                }

                if (new[] { row.Role, row.Status, row.Shown, row.Actions, row.Source }.Any(string.IsNullOrWhiteSpace))
                {
                    yield return $"EmptyCell:{row.Id}: a row says role, state, what is shown, which actions are active and the source (04 6.1.1).";
                }
            }
        }

        foreach (IGrouping<string, ScreenBlock> family in blocks.GroupBy(b => string.Join('.', b.Block.Split('.')[..2]), StringComparer.Ordinal))
        {
            if (!family.Select(b => b.Block).SequenceEqual(Enumerable.Range(1, family.Count()).Select(n => $"{family.Key}.{n}")))
            {
                yield return $"BlockNumbering:{family.Key}: the blocks are not numbered 1..{family.Count()} in order.";
            }
        }
    }

    // The pages: a screen's page is in the code and every page of the code is a screen's or a listed non-screen (04 4.4).
    private static IEnumerable<string> Pages(IReadOnlyDictionary<string, string> sources, Dictionary<string, string> routes)
    {
        HashSet<string> claimed = new(StringComparer.Ordinal);
        Dictionary<string, string> owner = new(StringComparer.Ordinal);
        foreach (ScreenEntry entry in StateRoleInventory.Screens)
        {
            foreach (string route in entry.Routes)
            {
                claimed.Add(route);
                if (!routes.ContainsValue(route))
                {
                    yield return $"PageMissing:{entry.Screen}: no page has the route '{route}'.";
                }
                else if (!owner.TryAdd(route, entry.Screen) && !StateRoleInventory.SharedRoutes.Contains(route, StringComparer.Ordinal))
                {
                    yield return $"RouteClaimedTwice:{entry.Screen}: the route '{route}' is also {owner[route]}'s.";
                }
            }

            if (entry.View is not null && !sources.ContainsKey(entry.View))
            {
                yield return $"PageMissing:{entry.Screen}: the view '{entry.View}' is not in the code.";
            }
        }

        foreach (NonScreenPage page in StateRoleInventory.NonScreenPages)
        {
            claimed.Add(page.Route);
            if (!routes.ContainsValue(page.Route))
            {
                yield return $"DeadNonScreenPage: no page has the route '{page.Route}'.";
            }
        }

        foreach ((string file, string route) in routes.OrderBy(r => r.Key, StringComparer.Ordinal))
        {
            if (!claimed.Contains(route))
            {
                yield return $"PageWithoutScreen: {file} ('{route}') is no screen's page and no listed non-screen page.";
            }
        }
    }

    // 04 6.1.5.2: the sales gate changes E-04, E-12, E-13 and the gate of the data-collecting entries E-10, E-20, E-22 - in the sentence, in the rows and in the code - and the panel shows it in E-31, E-44 to E-47.
    private static IEnumerable<string> Gates(string document, IReadOnlyDictionary<string, string> sources, Dictionary<string, string> routes, IReadOnlyList<ScreenBlock> blocks)
    {
        (IReadOnlyList<string> sales, IReadOnlyList<string> data) = StateRoleDocument.GateScreens(document);
        string[] salesRows = RowScreens(blocks, "6.2.", r => SalesState().IsMatch(r.Status) && !Same(r));
        string[] dataRows = RowScreens(blocks, "6.2.", r => DataState().IsMatch(r.Status) && !Same(r));
        string[] salesCode = CodeScreens(sources, routes, StateRoleInventory.SalesGateTokens, "Pages/Panel/");
        string[] dataCode = CodeScreens(sources, routes, StateRoleInventory.DataGateTokens, "Pages/Panel/");

        foreach ((string label, IReadOnlyList<string> expected, string[] rows, string[] code) in new[] { ("sales gate", sales, salesRows, salesCode), ("data gate", data, dataRows, dataCode) })
        {
            if (!rows.SequenceEqual(expected.Order(StringComparer.Ordinal)))
            {
                yield return $"GateRowsDiffer: the {label} changes [{string.Join(", ", expected)}] in 04 6.1.5.2, the rows that change the screen are [{string.Join(", ", rows)}].";
            }

            if (!code.SequenceEqual(expected.Order(StringComparer.Ordinal)))
            {
                yield return $"GateReadDiffers: the {label} is read by the pages of [{string.Join(", ", code)}], 04 6.1.5.2 says [{string.Join(", ", expected)}].";
            }
        }

        IReadOnlyList<string> panel = StateRoleDocument.PanelGateScreens(document);
        string[] panelCode = CodeScreens(sources, routes, ["GetSalesGateAsync"], null, "Pages/Panel/");
        foreach (string screen in panelCode.Where(s => !panel.Contains(s, StringComparer.Ordinal)))
        {
            yield return $"GateReadDiffers: the panel's {screen} reads the sales gate, 04 6.1.5.2 names [{string.Join(", ", panel)}].";
        }

        // D-1657: the other direction - a screen 04 6.1.5.2 names stops reading the gate, and the panel no longer shows it.
        foreach (string screen in panel.Where(s => !panelCode.Contains(s, StringComparer.Ordinal) && !PanelGateReadGaps.Contains(s, StringComparer.Ordinal)))
        {
            yield return $"GateReadMissing: 04 6.1.5.2 says the panel shows the sales gate in {screen}, its page does not read it.";
        }

        // The known gaps are held both ways: a gap that the page has closed (or that the document no longer names) is a stale entry.
        foreach (string screen in PanelGateReadGaps.Where(s => panelCode.Contains(s, StringComparer.Ordinal) || !panel.Contains(s, StringComparer.Ordinal)))
        {
            yield return $"GateReadGapStale: {screen} is listed as a screen whose page does not read the sales gate (D-1778), but its page reads it or 04 6.1.5.2 no longer names it.";
        }
    }

    // 04 6.1.5.1: an administrator row is only in the screens a draft preview or the "Taslak" label changes; E-05's row says the screen is the same.
    private static IEnumerable<string> Administrator(string document, IReadOnlyDictionary<string, string> sources, Dictionary<string, string> routes, IReadOnlyList<ScreenBlock> blocks)
    {
        (IReadOnlyList<string> changed, IReadOnlyList<string> same) = StateRoleDocument.AdministratorScreens(document);
        string[] rowScreens = RowScreens(blocks, "6.2.", r => r.Role.Contains("Yönetici", StringComparison.Ordinal));
        string[] allowed = [.. changed.Concat(same).Order(StringComparer.Ordinal)];
        foreach (string screen in rowScreens.Except(allowed, StringComparer.Ordinal))
        {
            yield return $"AdministratorRowOutsideList:{screen}: the screen has an administrator row, 04 6.1.5.1 names [{string.Join(", ", allowed)}].";
        }

        foreach (string screen in allowed.Except(rowScreens, StringComparer.Ordinal))
        {
            yield return $"AdministratorRowMissing:{screen}: 04 6.1.5.1 names the screen, its block has no administrator row.";
        }

        foreach (ScreenBlock block in blocks.Where(b => same.Contains(b.Screen, StringComparer.Ordinal)))
        {
            foreach (MatrixRow row in block.Rows.Where(r => r.Role.Contains("Yönetici", StringComparison.Ordinal) && !Same(r)))
            {
                yield return $"AdministratorRowChanges:{row.Id}: 04 6.1.5.1 says the screen is the same for the administrator, the row shows something else.";
            }
        }

        string[] code = CodeScreens(sources, routes, StateRoleInventory.DraftTokens, "Pages/Panel/");
        string[] expected = [.. changed.Except(StateRoleInventory.DraftConsequenceScreens, StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        if (!code.SequenceEqual(expected))
        {
            yield return $"DraftReadDiffers: the pages that open a draft for the administrator are [{string.Join(", ", code)}], 04 6.1.5.1 names [{string.Join(", ", expected)}] besides the consequence screens.";
        }

        foreach (string screen in StateRoleInventory.DraftConsequenceScreens.Except(changed, StringComparer.Ordinal))
        {
            yield return $"DraftConsequenceUnlisted:{screen}: the inventory counts the screen as a consequence of the draft, 04 6.1.5.1 does not name it.";
        }
    }

    // 04 6.1.8: the order's two axes and the publish and request statuses are named as 02 5 names them, and the code's badge text is that name.
    private static IEnumerable<string> StatusNames(IReadOnlyList<ScreenBlock> blocks, string requirements)
    {
        string[] shipment = [.. Names(requirements, "### 5.4 ").Select(n => n.Name)];
        string[] payment = [.. Names(requirements, "### 5.5 ").Select(n => n.Name)];
        (string Name, string Code)[] defect = [.. Names(requirements, "### 5.10 ")];
        (string Name, string Code)[] contact = [.. Names(requirements, "### 5.13 ")];
        foreach (string line in Differences<OrderStatus>(requirements, "### 5.4 ", StatusBadges.For).Concat(Differences<PaymentStatus>(requirements, "### 5.5 ", StatusBadges.For))
            .Concat(Differences<PublishStatus>(requirements, "### 5.1 ", StatusBadges.For)).Concat(Differences<DefectClaimStatus>(requirements, "### 5.10 ", StatusBadges.For))
            .Concat(Differences<ContactRequestStatus>(requirements, "### 5.13 ", StatusBadges.For)))
        {
            yield return line;
        }

        string[] defectNames = [.. defect.Select(d => d.Name)];
        string[] contactNames = [.. contact.Select(d => d.Name)];
        foreach (ScreenBlock block in blocks)
        {
            foreach (MatrixRow row in block.Rows)
            {
                int at = 0;
                while ((at = row.Status.IndexOf(" + ", at, StringComparison.Ordinal)) >= 0)
                {
                    string left = row.Status[..at];
                    string right = row.Status[(at + 3)..];
                    if (!shipment.Any(n => EndsWithWord(left, n)))
                    {
                        yield return $"StatusNameDiffers:{row.Id}: the word before '+' in '{row.Status}' is not an order status of 02 5.4 ({string.Join(", ", shipment)}).";
                    }

                    if (!payment.Any(n => StartsWithWord(right, n)))
                    {
                        yield return $"StatusNameDiffers:{row.Id}: the word after '+' in '{row.Status}' is not a payment status of 02 5.5 ({string.Join(", ", payment)}).";
                    }

                    at += 3;
                }

                foreach (Match claim in ClaimStatus().Matches(row.Status))
                {
                    string[] names = claim.Groups[1].Value.StartsWith("ayıp", StringComparison.OrdinalIgnoreCase) ? defectNames : contactNames;
                    if (!names.Any(n => StartsWithWord(claim.Groups[2].Value, n)) && !ClaimWords.Contains(claim.Groups[2].Value.Split(' ')[0], StringComparer.Ordinal))
                    {
                        yield return $"StatusNameDiffers:{row.Id}: '{claim.Value}' is not a request status of 02 5.10 or 5.13 ({string.Join(", ", names)}).";
                    }
                }

                foreach (string word in Synonyms.Where(s => ContainsWord(row.Status, s)))
                {
                    yield return $"StatusNameDiffers:{row.Id}: '{word}' is not a status name of 02 5; the document names the status as 02 5 does.";
                }
            }
        }
    }

    // 04 6.1.8: every operation of 03 1.11 is the source of at least one row; the two the document says do not exist are named by a row that says they are not presented.
    private static IEnumerable<string> Operations(string document, IReadOnlyList<ScreenBlock> blocks)
    {
        HashSet<int> named = [];
        foreach (MatrixRow row in blocks.SelectMany(b => b.Rows))
        {
            foreach (Match range in OperationRange().Matches(row.Source))
            {
                int from = int.Parse(range.Groups[1].Value, CultureInfo.InvariantCulture);
                int to = range.Groups[2].Success ? int.Parse(range.Groups[2].Value, CultureInfo.InvariantCulture) : from;
                named.UnionWith(Enumerable.Range(from, to - from + 1));
            }
        }

        int[] catalog = [.. OrderOperationCatalog.Rows.Select(r => int.Parse(r.Id[5..], CultureInfo.InvariantCulture))];
        foreach (int missing in catalog.Except(named).Order())
        {
            yield return $"OperationNotInMatrix:1.11.{missing}: no row's source names the operation (04 6.1.8).";
        }

        foreach (int unknown in named.Except(catalog).Order())
        {
            yield return $"OperationUnknown:1.11.{unknown}: a row's source names an operation the decision table does not have.";
        }

        Match absent = AbsentOperations().Match(string.Join('\n', DataModelDocument.Section(document, "### 6.1 ").Split('\n').Where(l => l.StartsWith("**6.1.8", StringComparison.Ordinal))));
        if (!absent.Success)
        {
            yield return "OperationNotInMatrix: 04 6.1.8 no longer says which two operations do not exist.";
            yield break;
        }

        foreach (int number in new[] { int.Parse(absent.Groups[1].Value, CultureInfo.InvariantCulture), int.Parse(absent.Groups[2].Value, CultureInfo.InvariantCulture) })
        {
            bool said = blocks.SelectMany(b => b.Rows).Any(r => OperationRange().Matches(r.Source).Any(m => m.Groups[1].Value == number.ToString(CultureInfo.InvariantCulture)) && NotPresented(r));
            if (!said)
            {
                yield return $"OperationNotInMatrix:1.11.{number}: no row says the operation is not presented (04 6.1.4: it is not there, not disabled).";
            }
        }
    }

    // 04 6.1.3 and 02 10.1: the panel is one role; every panel page, form and operation is under /panel and the folder is closed by the admin policy.
    private static IEnumerable<string> Panel(IReadOnlyDictionary<string, string> sources, Dictionary<string, string> routes, IReadOnlyList<ScreenBlock> blocks)
    {
        foreach (MatrixRow row in blocks.Where(b => b.Block.StartsWith("6.3.", StringComparison.Ordinal)).SelectMany(b => b.Rows))
        {
            string[] roles = [.. row.Role.Split('·').Select(r => r.Trim())];
            if (roles.Any(r => r is not ("Yönetici" or "Kullanıcı")))
            {
                yield return $"PanelNotSingleRole:{row.Id}: the panel's roles are Yönetici and Kullanıcı, the row says '{row.Role}'.";
            }
        }

        foreach (ScreenEntry entry in StateRoleInventory.Screens)
        {
            bool panelBlock = entry.Block.StartsWith("6.3.", StringComparison.Ordinal);
            foreach (string route in entry.Routes.Where(r => r.StartsWith("/panel", StringComparison.Ordinal) != panelBlock))
            {
                yield return $"PanelPageOutsidePanel:{entry.Screen}: the route '{route}' is {(panelBlock ? "a panel screen's outside /panel" : "a customer screen's under /panel")}.";
            }
        }

        foreach ((string file, string route) in routes)
        {
            bool inFolder = file.StartsWith("Pages/Panel/", StringComparison.Ordinal);
            if (inFolder != (route == "/panel" || route.StartsWith("/panel/", StringComparison.Ordinal)))
            {
                yield return $"PanelPageOutsidePanel: {file} has the route '{route}'; the pages of the panel folder are under /panel and no page outside it is (the folder is what the admin policy closes).";
            }
        }

        if (!sources.TryGetValue("Infrastructure/Security/SessionAccessSetup.cs", out string? setup) || !setup.Contains("AuthorizeFolder(SessionSchemes.PanelFolder, SessionSchemes.AdminPolicy)", StringComparison.Ordinal))
        {
            yield return "PanelFolderNotClosed: the panel folder is not closed by AuthorizeFolder(PanelFolder, AdminPolicy).";
        }

        if (!sources.TryGetValue("Infrastructure/Security/SessionSchemes.cs", out string? schemes) || !schemes.Contains("PanelFolder = \"/Panel\"", StringComparison.Ordinal))
        {
            yield return "PanelFolderNotClosed: SessionSchemes.PanelFolder is not '/Panel'.";
        }

        foreach ((string file, string text) in sources.Where(s => s.Key.EndsWith(".cshtml", StringComparison.Ordinal)).OrderBy(s => s.Key, StringComparer.Ordinal))
        {
            bool panelFile = file.StartsWith("Pages/Panel/", StringComparison.Ordinal) || Path.GetFileName(file).StartsWith("_Panel", StringComparison.Ordinal);
            foreach (Match form in FormTag().Matches(text))
            {
                foreach (Match address in FormAddress().Matches(form.Value))
                {
                    string target = address.Groups[2].Value;
                    bool toPanel = target.StartsWith("/panel", StringComparison.OrdinalIgnoreCase) || target.StartsWith("@PanelFramePaths", StringComparison.Ordinal);
                    if (panelFile && target.StartsWith('/') && !toPanel)
                    {
                        yield return $"PanelFormOutsidePanel: {file} posts a panel form to '{target}'.";
                    }

                    if (!panelFile && toPanel)
                    {
                        yield return $"PanelFormOutsidePanel: {file} posts a form to the panel ('{target}') from outside the panel folder.";
                    }
                }
            }
        }
    }

    private static readonly string[] ClaimWords = ["yok", "kaydı", "ekranı", "ekranında", "formu", "durumu"];

    /// <summary>The lines of a two-directional comparison with the first word of each line the label of what differs.</summary>
    private static IEnumerable<string> Relabel(string label, IEnumerable<string> documented, IEnumerable<string> model, string onlyInDocument, string onlyInModel)
    {
        return SetComparison.Compare(label, documented, model)
            .Select(l => l.Replace(label + ": only in the document: ", onlyInDocument + ":", StringComparison.Ordinal).Replace(label + ": only in the model: ", onlyInModel + ":", StringComparison.Ordinal));
    }

    /// <summary>A row says an operation is not presented: "Sunulmaz" or "sunulmaz" in what is shown or the actions, or the operation "yoktur" (04 6.1.4).</summary>
    private static bool NotPresented(MatrixRow row) =>
        new[] { row.Shown, row.Actions }.Any(c => c.Contains("unulmaz", StringComparison.Ordinal) || c.Contains("yoktur", StringComparison.Ordinal));

    private static bool Same(MatrixRow row) => SameWords.Any(w => row.Shown.Contains(w, StringComparison.Ordinal));

    private static string[] RowScreens(IReadOnlyList<ScreenBlock> blocks, string family, Func<MatrixRow, bool> match)
    {
        return [.. blocks.Where(b => b.Block.StartsWith(family, StringComparison.Ordinal) && b.Rows.Any(match)).Select(b => b.Screen).Order(StringComparer.Ordinal)];
    }

    /// <summary>The screens of the pages (and of the support files the inventory names) whose files hold one of the tokens; <paramref name="skipFolder"/> leaves the panel out, <paramref name="onlyFolder"/> keeps only it.</summary>
    private static string[] CodeScreens(IReadOnlyDictionary<string, string> sources, Dictionary<string, string> routes, IReadOnlyList<string> tokens, string? skipFolder, string? onlyFolder = null)
    {
        HashSet<string> screens = new(StringComparer.Ordinal);
        foreach ((string file, string text) in sources.Where(s => s.Key.StartsWith("Pages/", StringComparison.Ordinal)))
        {
            if (StateRoleInventory.SharedFolders.Any(f => file.StartsWith(f, StringComparison.Ordinal)) || (skipFolder is not null && file.StartsWith(skipFolder, StringComparison.Ordinal)) || (onlyFolder is not null && !file.StartsWith(onlyFolder, StringComparison.Ordinal)))
            {
                continue;
            }

            if (!tokens.Any(t => text.Contains(t, StringComparison.Ordinal)))
            {
                continue;
            }

            screens.Add(ScreenOf(file, routes) ?? "(" + file + ")");
        }

        return [.. screens.Order(StringComparer.Ordinal)];
    }

    /// <summary>The screen a file belongs to: the first screen that claims the route of its page, or the screen the inventory gives a support file; null when it is no screen's.</summary>
    private static string? ScreenOf(string file, Dictionary<string, string> routes)
    {
        if (StateRoleInventory.SupportFiles.TryGetValue(file, out string? support))
        {
            return support;
        }

        string page = file.EndsWith(".cshtml.cs", StringComparison.Ordinal) ? file[..^3] : file;
        return routes.TryGetValue(page, out string? route) ? StateRoleInventory.Screens.FirstOrDefault(s => s.Routes.Contains(route, StringComparer.Ordinal))?.Screen : null;
    }

    private static IEnumerable<(string Name, string Code)> Names(string requirements, string heading)
    {
        IReadOnlyList<string[]> table = DataModelDocument.Tables(DataModelDocument.Section(requirements, heading))[0];
        return table.Select(cells => (cells[0], cells[1].Trim('`')));
    }

    private static IEnumerable<string> Differences<T>(string requirements, string heading, Func<T, Badge> badge)
        where T : struct, Enum
    {
        (string Name, string Code)[] documented = [.. Names(requirements, heading)];
        foreach (string line in SetComparison.Compare($"02 {heading.Trim('#', ' ')} codes", documented.Select(d => d.Code), Enum.GetNames<T>()))
        {
            yield return "StatusNameDiffers: " + line;
        }

        foreach ((string name, string code) in documented.Where(d => Enum.TryParse(d.Code, out T _)))
        {
            T value = Enum.Parse<T>(code);
            if (!string.Equals(badge(value).Text, name, StringComparison.Ordinal))
            {
                yield return $"StatusNameDiffers: 02 {heading.Trim('#', ' ')} names {code} '{name}', the badge of the code says '{badge(value).Text}'.";
            }
        }
    }

    private static bool EndsWithWord(string text, string word) => text.EndsWith(word, StringComparison.Ordinal) && (text.Length == word.Length || !char.IsLetter(text[text.Length - word.Length - 1]));

    private static bool StartsWithWord(string text, string word) => text.StartsWith(word, StringComparison.Ordinal) && (text.Length == word.Length || !char.IsLetter(text[word.Length]));

    private static bool ContainsWord(string text, string word)
    {
        for (int at = text.IndexOf(word, StringComparison.Ordinal); at >= 0; at = text.IndexOf(word, at + 1, StringComparison.Ordinal))
        {
            bool start = at == 0 || !char.IsLetter(text[at - 1]);
            bool end = at + word.Length == text.Length || !char.IsLetter(text[at + word.Length]);
            if (start && end)
            {
                return true;
            }
        }

        return false;
    }

    [GeneratedRegex(@"\A@page\s+""([^""]+)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PageDirective();

    [GeneratedRegex(@"(\d+) ekranın her biri", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScreenCount();

    [GeneratedRegex(@"satış (açık|kapalı|kapandı)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SalesState();

    [GeneratedRegex(@"veri toplayan girişlerin kapısı|Hâl: kapı (açık|kapalı)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DataState();

    [GeneratedRegex(@"\b([Aa]yıp|[İi]letişim) talebi (\p{L}+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ClaimStatus();

    [GeneratedRegex(@"1\.11\.(\d+)(?:–§1\.11\.(\d+))?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex OperationRange();

    [GeneratedRegex(@"`03 §1\.11\.(\d+)` ve §1\.11\.(\d+) ""yoktur""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AbsentOperations();

    [GeneratedRegex(@"<form\b[^>]*>", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FormTag();

    [GeneratedRegex(@"\b(action|asp-page)=""([^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FormAddress();
}
