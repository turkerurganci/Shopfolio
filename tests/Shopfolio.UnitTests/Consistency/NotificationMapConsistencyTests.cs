using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the notification map of the code equal to the notification map of 03 in both directions (T328 criteria 1, 2, 3, 7; KK1, KK3, KK6, KK7; 03 7; 09 21.4 item 1): the
/// three tables of 03 7.1, 7.2 and 7.3 against <see cref="NotificationMap"/> row by row, the dispatch types against the notices of the document, the address kind of every
/// trigger against its queue path, and the notices that the flow tables name against the notices that the map's "Akış" column ties to them. Reads <c>Docs/03_USER_FLOWS.md</c>
/// and the application assembly only. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed class NotificationMapConsistencyTests
{
    /// <summary>
    /// The only flow rows whose notice cell does not carry the code of a notice that a 7.1 row ties to it, each with its reason; a row that stops being needed fails the test (<see cref="TheExceptions_AreStillNeeded"/>).
    /// Empty - D-295 is closed; a new exception comes in with its reason.
    /// </summary>
    private static readonly (string TriggerRow, string FlowRow)[] FlowRowsWithoutTheCode = [];

    /// <summary>
    /// The only flow rows that name a notice no 7.1 row ties to them, each with its reason. Empty - D-295 is closed; a new exception comes in with its reason.
    /// </summary>
    private static readonly (string FlowRow, DispatchType Notice)[] NoticesNoTriggerRowCites = [];

    /// <summary>The sections of 03 whose flow rows the map's "Akış" column cites; their notice cells are compared row by row. §1 (the record tables) and §3 (the error rows, which the map ties to the main flow's row) are compared by notice only.</summary>
    private static readonly string[] SectionsOfTheMapsFlow = ["2", "4", "5", "6", "8", "9", "10"];

    [Fact]
    public void TheTriggers_EqualTheRowsOf7_1_InBothDirections()
    {
        IEnumerable<string> documented = NotificationMapDocument.Triggers().Select(r => $"{r.Id} | {NotificationMapDocument.TypeOf(r)?.ToString() ?? "B1…B16"} | {NotificationMapDocument.AddresseeOf(r)}");
        IEnumerable<string> model = NotificationMap.Triggers.Select(t => $"{t.RowId} | {t.Type?.ToString() ?? "B1…B16"} | {t.Addressee}");

        Assert.Empty(SetComparison.Compare("03 7.1 trigger", documented, model));
    }

    [Fact]
    public void TheNoNoticeRows_EqualTheRowsOf7_2_WithTheirSources_InBothDirections()
    {
        IEnumerable<string> documented = NotificationMapDocument.NoNotices().Select(r => $"{r.Id} | {string.Join("; ", NotificationMapDocument.SourcesOf(r.Cells[3]))}");
        IEnumerable<string> model = NotificationMap.NoNotices.Select(n => $"{n.RowId} | {string.Join("; ", n.Sources)}");

        Assert.Empty(SetComparison.Compare("03 7.2 row", documented, model));
        Assert.All(NotificationMap.NoNotices, n => Assert.NotEmpty(n.Sources));
    }

    [Fact]
    public void TheEvents_EqualTheRowsOf7_3_WithTheRowsTheyName_InBothDirections()
    {
        IEnumerable<string> documented = NotificationMapDocument.Events().Select(r => $"{r.Id} | {string.Join("; ", NotificationMapDocument.RowsNamedBy(r.Cells[3]))}");
        IEnumerable<string> model = NotificationMap.Events.Select(e => $"{e.RowId} | {string.Join("; ", e.Rows)}");

        Assert.Empty(SetComparison.Compare("03 7.3 row", documented, model));
    }

    [Fact]
    public void EveryRowOf7_3_NamesOnlyRowsThatExist_AndEveryRowOf7_1_IsNamedByARowOf7_3()
    {
        HashSet<string> known = [.. NotificationMap.Triggers.Select(t => t.RowId), .. NotificationMap.NoNotices.Select(n => n.RowId)];
        string[] named = [.. NotificationMap.Events.SelectMany(e => e.Rows).Distinct()];

        Assert.All(NotificationMap.Events, e => Assert.NotEmpty(e.Rows));
        Assert.Empty(named.Except(known));

        // Every notice row is named by an event row. The 7.2 rows that no event row names are the standing statements ("no notice for X", e.g. the time-outs, the contact form, the
        // marketing messages) that no state-machine transition or record event leads to; the 7.2 rows an event row names are checked above.
        Assert.Empty(NotificationMap.Triggers.Select(t => t.RowId).Except(named));
    }

    [Fact]
    public void EveryTransition_HasARowOf7_3_AndEveryTransitionRowOf7_3_IsATransition()
    {
        // 7.3 names a transition by its code ("S5", "Ö1", "Ö3–Ö5"); the code names of the enum write Ö as O (06 5.2.6). Correction is not a transition of its own (09 10.1.3 item 8).
        List<string> codesOf7_3 = [];
        foreach (DocumentRow row in NotificationMapDocument.Events())
        {
            string cell = row.Cells[1];
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(cell, @"^(S\d+|Ö\d)(?:–(Ö\d))?\b", System.Text.RegularExpressions.RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
            if (!match.Success)
            {
                continue;
            }

            string first = match.Groups[1].Value;
            codesOf7_3.Add(first);
            if (match.Groups[2].Success)
            {
                int from = int.Parse(first[1..], System.Globalization.CultureInfo.InvariantCulture);
                int to = int.Parse(match.Groups[2].Value[1..], System.Globalization.CultureInfo.InvariantCulture);
                codesOf7_3.AddRange(Enumerable.Range(from + 1, to - from).Select(n => $"Ö{n}"));
            }
        }

        IEnumerable<string> model = Enum.GetValues<Shopfolio.Modules.Ordering.Contracts.TransitionCode>()
            .Where(c => c != Shopfolio.Modules.Ordering.Contracts.TransitionCode.Correction)
            .Select(c => c.ToString().Replace('O', 'Ö'));

        Assert.Empty(SetComparison.Compare("03 7.3 transition", codesOf7_3, model));
    }

    [Fact]
    public void ARowOf7_3_ThatSaysNoNotice_NamesNoRowOf7_1()
    {
        foreach (DocumentRow row in NotificationMapDocument.Events().Where(r => r.Cells[2].StartsWith("Yok", StringComparison.Ordinal) && !r.Cells[2].Contains("B-", StringComparison.Ordinal) && !r.Cells[2].Contains("F-", StringComparison.Ordinal)))
        {
            Assert.DoesNotContain(NotificationMapDocument.RowsNamedBy(row.Cells[3]), id => id.StartsWith("7.1.", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void TheDispatchTypes_AreExactlyTheNoticesOf7_1_AndEveryTypeIsOnOneQueuePath()
    {
        IEnumerable<string> documented = NotificationMapDocument.Triggers().Select(NotificationMapDocument.TypeOf).OfType<DispatchType>().Distinct().Select(t => t.ToString());
        IEnumerable<string> model = Enum.GetValues<DispatchType>().Select(t => t.ToString());

        Assert.Empty(SetComparison.Compare("03 7.1 notice", documented, model));
        Assert.All(Enum.GetValues<DispatchType>(), type => Assert.NotEmpty(NotificationMap.AddresseesOf(type)));
        Assert.All(Enum.GetValues<DispatchType>(), type => NotificationMap.Require(type, NotificationMap.FamilyOf(type)));
    }

    [Fact]
    public void EveryTrigger_AddressesTheKindOfAddressItsQueuePathHas()
    {
        Dictionary<NoticeFamily, NoticeAddressee[]> byFamily = new()
        {
            [NoticeFamily.Customer] = [NoticeAddressee.OrderContact, NoticeAddressee.CorrectedOrderContact, NoticeAddressee.OrderContactBeforeCorrection, NoticeAddressee.CurrentOrderContact],
            [NoticeFamily.Firm] = [NoticeAddressee.FirmContact],
            [NoticeFamily.Administrators] = [NoticeAddressee.EachAdministrator],
            [NoticeFamily.Account] =
            [
                NoticeAddressee.RegisteredAddress, NoticeAddressee.AccountAddress, NoticeAddressee.AccountAddressOrOldOnRevert, NoticeAddressee.NewAddress,
                NoticeAddressee.OldAccountAddress, NoticeAddressee.InviteeAddress, NoticeAddressee.DeletedAccountAddress,
            ],
        };

        foreach (NoticeTrigger trigger in NotificationMap.Triggers)
        {
            if (trigger.Type is { } type)
            {
                Assert.Contains(trigger.Addressee, byFamily[NotificationMap.FamilyOf(type)]);
            }
        }

        // The two address rules that 03 7.1 spells out: B-15 goes to the address before the correction, F-5 and F-6 to every administrator's own address.
        Assert.All(NotificationMap.Triggers.Where(t => t.Type == DispatchType.B15), t => Assert.Equal(NoticeAddressee.OrderContactBeforeCorrection, t.Addressee));
        Assert.All(NotificationMap.Triggers.Where(t => t.Type is DispatchType.F5 or DispatchType.F6), t => Assert.Equal(NoticeAddressee.EachAdministrator, t.Addressee));
        Assert.All(NotificationMap.Triggers.Where(t => t.Type is DispatchType.F1 or DispatchType.F2 or DispatchType.F3 or DispatchType.F4), t => Assert.Equal(NoticeAddressee.FirmContact, t.Addressee));
        Assert.All(NotificationMap.Triggers.Where(t => t.Type is null), t => Assert.Equal(NoticeAddressee.CurrentOrderContact, t.Addressee));
    }

    [Fact]
    public void EveryNoticeOf7_1_GoesByEmail_TheOnlyChannel()
    {
        Assert.NotEmpty(NotificationMapDocument.Triggers());
        Assert.All(NotificationMapDocument.Triggers(), r => Assert.StartsWith(NotificationMapDocument.ChannelPrefix, r.Cells[6], StringComparison.Ordinal));
    }

    [Fact]
    public void TheNoticesOfTheFlowTables_AndTheNoticesTheMapTiesToThem_AreTheSame_InBothDirections()
    {
        IReadOnlyDictionary<string, string> flow = NotificationMapDocument.FlowNoticeCells();
        IReadOnlySet<string> allRows = NotificationMapDocument.AllRowIds();
        Dictionary<string, DocumentRow> documentRows = NotificationMapDocument.Triggers().ToDictionary(r => r.Id, StringComparer.Ordinal);
        List<string> problems = [];

        // Map to flow: every flow row a trigger cites is a row of 03, and where its table has a notice column the cell names the trigger's notice.
        Dictionary<string, HashSet<DispatchType>> tied = new(StringComparer.Ordinal);
        HashSet<string> tiedToTheResend = new(StringComparer.Ordinal);
        foreach (NoticeTrigger trigger in NotificationMap.Triggers)
        {
            foreach (string flowRow in NotificationMapDocument.FlowRowsNamedBy(documentRows[trigger.RowId].Cells[3]))
            {
                if (!allRows.Contains(flowRow))
                {
                    problems.Add($"{trigger.RowId} cites {flowRow}, which is not a row of 03.");
                    continue;
                }

                if (trigger.Type is null)
                {
                    tiedToTheResend.Add(flowRow);
                    continue;
                }

                if (!tied.TryGetValue(flowRow, out HashSet<DispatchType>? set))
                {
                    tied[flowRow] = set = [];
                }

                set.Add(trigger.Type.Value);
                if (flow.TryGetValue(flowRow, out string? cell)
                    && IsBOrF(trigger.Type.Value)
                    && !NotificationMapDocument.NoticesOf(cell).Contains(trigger.Type.Value)
                    && !FlowRowsWithoutTheCode.Contains((trigger.RowId, flowRow)))
                {
                    problems.Add($"{trigger.RowId} ties {trigger.Type} to {flowRow}, whose notice cell does not name it: {cell}");
                }
            }
        }

        // Flow to map: every notice a flow row names is tied to it by a trigger row. The flow rows of the map's own sections are compared row by row; §1 and §3 by notice.
        HashSet<DispatchType> mapNotices = [.. NotificationMap.Triggers.Where(t => t.Type is not null).Select(t => t.Type!.Value)];
        foreach ((string flowRow, string cell) in flow)
        {
            foreach (DispatchType notice in NotificationMapDocument.NoticesOf(cell))
            {
                string section = flowRow[..flowRow.IndexOf('.', StringComparison.Ordinal)];
                if (!mapNotices.Contains(notice))
                {
                    problems.Add($"{flowRow} names {notice}, which the map has no trigger row for.");
                }
                else if (SectionsOfTheMapsFlow.Contains(section)
                    && !(tied.TryGetValue(flowRow, out HashSet<DispatchType>? set) && set.Contains(notice))
                    && !(tiedToTheResend.Contains(flowRow) && IsCustomerNotice(notice))
                    && !NoticesNoTriggerRowCites.Contains((flowRow, notice)))
                {
                    problems.Add($"{flowRow} names {notice}, which no trigger row of the map ties to it: {cell}");
                }
            }
        }

        Assert.Empty(problems);
    }

    [Fact]
    public void TheExceptions_AreStillNeeded()
    {
        IReadOnlyDictionary<string, string> flow = NotificationMapDocument.FlowNoticeCells();
        Dictionary<string, DocumentRow> documentRows = NotificationMapDocument.Triggers().ToDictionary(r => r.Id, StringComparer.Ordinal);

        foreach ((string triggerRow, string flowRow) in FlowRowsWithoutTheCode)
        {
            NoticeTrigger trigger = Assert.Single(NotificationMap.Triggers, t => t.RowId == triggerRow);
            Assert.Contains(flowRow, NotificationMapDocument.FlowRowsNamedBy(documentRows[triggerRow].Cells[3]));
            Assert.DoesNotContain(trigger.Type!.Value, NotificationMapDocument.NoticesOf(flow[flowRow]));
        }

        foreach ((string flowRow, DispatchType notice) in NoticesNoTriggerRowCites)
        {
            Assert.Contains(notice, NotificationMapDocument.NoticesOf(flow[flowRow]));
            Assert.DoesNotContain(
                NotificationMap.Triggers,
                t => t.Type == notice && NotificationMapDocument.FlowRowsNamedBy(documentRows[t.RowId].Cells[3]).Contains(flowRow));
        }
    }

    private static bool IsBOrF(DispatchType type) => NotificationMap.FamilyOf(type) != NoticeFamily.Account;

    private static bool IsCustomerNotice(DispatchType type) => NotificationMap.FamilyOf(type) == NoticeFamily.Customer;
}
