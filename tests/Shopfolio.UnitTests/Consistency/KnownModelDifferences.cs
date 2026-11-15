using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>One difference between a model table of 07 5 and the code that is open as a backlog item: the exact line the check reports and the item that owns it.</summary>
internal sealed record KnownDifference(string Line, string Backlog);

/// <summary>
/// The differences between the model tables of 07 5 and the code that were found when the checks of T538 were first run and are not closed in this task (09 21.4 item 1:
/// "doküman kazanır ya da fark geri besleme tablosuna girer"). Each entry is one exact line of a check, not a pattern, and carries the backlog item that owns it. The entry is
/// no exemption that can grow silently: a check fails when its line is no longer reported (the difference is closed - delete the entry), when the backlog item is not a row of
/// <c>Docs/DEFERRED_BACKLOG.md</c> or <c>Docs/DEFERRED_BACKLOG.d/</c>, and - as always - when a new difference appears.
/// </summary>
internal static class KnownModelDifferences
{
    public static readonly IReadOnlyList<KnownDifference> Form =
    [
        // D-1606: UC-132, UC-136 and UC-137 are bound by the classes of UC-131 and UC-135 (07 5.2.2 gives each its own row and fields); the refund handlers also read the refund's key and the shown IBAN.
        new("Model: only in the document: MarkBankTransferRefundFailedInput", "D-1606"),
        new("Model: only in the document: SwitchRefundToBankTransferInput", "D-1606"),
        new("Field:ProcessRefundInput: only in the model: RefundId", "D-1606"),
        new("Field:ProcessRefundInput: only in the model: ShownIban", "D-1606"),

        // D-1333: the four boxes of the confirmation component are not 07 5.2's AcceptedCheckboxes.
        new("Field:PlaceOrderInput: only in the document: AcceptedCheckboxes", "D-1333"),
        new("Field:PlaceOrderInput: only in the model: AcceptPreInformation", "D-1333"),
        new("Field:PlaceOrderInput: only in the model: AcceptDistanceSales", "D-1333"),
        new("Field:PlaceOrderInput: only in the model: AcceptDigital", "D-1333"),
        new("Field:PlaceOrderInput: only in the model: AcceptService", "D-1333"),

        // D-1607: the image file of UC-117 is read from the request's file part, not from a model field.
        new("Field:SaveProductInput: only in the document: ImageFile", "D-1607"),
        new("Field:SaveContentInput: only in the document: ImageFile", "D-1607"),
        new("Field:SaveBrandingInput: only in the document: ImageFile", "D-1607"),

        // D-1527 and D-1551: the address correction reads one address type; the arrival-date correction names the record ArrivalRecordId.
        new("Field:CorrectAddressInput: only in the model: AddressType", "D-1527"),
        new("Field:CorrectArrivalDateInput: only in the document: ReturnId", "D-1551"),
        new("Field:CorrectArrivalDateInput: only in the model: ArrivalRecordId", "D-1551"),

        // D-1538: the removal marks of the brand and the content form.
        new("Field:SaveContentInput.LinkedProducts: only in the model: Remove", "D-1538"),
        new("Field:SaveBrandingInput: only in the model: RemoveLogo", "D-1538"),
        new("Field:SaveBrandingInput: only in the model: RemoveFavicon", "D-1538"),

        // D-1608: screen-own field names that 07 5.2.2 spells differently (the shown switch, the homepage version) or does not list (the product search's add mark).
        new("Field:SaveContentInput: only in the model: AddProducts", "D-1608"),
        new("Field:SaveHomepageInput: only in the document: HomepageVersion", "D-1608"),
        new("Field:SaveHomepageInput: only in the model: Version", "D-1608"),
        new("Field:SuspendSalesInput: only in the document: OriginalIsSalesTemporarilyClosed", "D-1608"),
        new("Field:SuspendSalesInput: only in the model: Original", "D-1608"),
        new("Field:ResumeSalesInput: only in the document: OriginalIsSalesTemporarilyClosed", "D-1608"),
        new("Field:ResumeSalesInput: only in the model: Original", "D-1608"),
    ];

    public static readonly IReadOnlyList<KnownDifference> Query =
    [
        // D-1609: the handlers of these screens read their parameters as plain values; 07 5.3.2 names a query model for each.
        new("Model: only in the document: RegisterQuery", "D-1609"),
        new("Model: only in the document: SignInQuery", "D-1609"),
        new("Model: only in the document: ReauthenticateQuery", "D-1609"),
        new("Model: only in the document: AccountQuery", "D-1609"),
        new("Model: only in the document: AdminSignInQuery", "D-1609"),

        // D-1205: the category list has its own query class beside ProductQuery.
        new("Model: only in the model: CategoryQuery", "D-1205"),
    ];

    public static readonly IReadOnlyList<KnownDifference> Handlers =
    [
        // D-1609: plain values where 07 names a query model (and the Google return, which 07 5.3.2 has no row for).
        new("InputWithoutModel:UC-47:RegisterModel.OnGetAsync(pendingRegistration)", "D-1609"),
        new("InputWithoutModel:UC-51:GoogleReturnModel.OnGetAsync(code)", "D-1609"),
        new("InputWithoutModel:UC-51:GoogleReturnModel.OnGetAsync(state)", "D-1609"),
        new("InputWithoutModel:UC-51:GoogleReturnModel.OnGetAsync(error)", "D-1609"),
        new("InputWithoutModel:UC-54:SignInModel.OnGetAsync(pendingRegistration)", "D-1609"),
        new("ModelNotBound:UC-47:RegisterQuery", "D-1609"),
        new("ModelWithoutEndpoint:RegisterQuery", "D-1609"),
        new("ModelNotBound:UC-54:SignInQuery", "D-1609"),
        new("ModelWithoutEndpoint:SignInQuery", "D-1609"),
        new("ModelNotBound:UC-60:ReauthenticateQuery", "D-1609"),
        new("ModelNotBound:UC-108:ReauthenticateQuery", "D-1609"),
        new("ModelWithoutEndpoint:ReauthenticateQuery", "D-1609"),
        new("ModelNotBound:UC-63:AccountQuery/AdminAccountQuery", "D-1609"),
        new("ModelNotBound:UC-104:AccountQuery/AdminAccountQuery", "D-1609"),
        new("ModelWithoutEndpoint:AccountQuery/AdminAccountQuery", "D-1609"),
        new("ModelNotBound:UC-84:AdminSignInQuery", "D-1609"),
        new("ModelWithoutEndpoint:AdminSignInQuery", "D-1609"),

        // D-1606: the refund handlers share classes.
        new("ModelNotBound:UC-132:MarkBankTransferRefundFailedInput", "D-1606"),
        new("ModelWithoutEndpoint:MarkBankTransferRefundFailedInput", "D-1606"),
        new("ModelNotBound:UC-136:SwitchRefundToBankTransferInput/MarkCardRefundFailedInput", "D-1606"),
        new("ModelNotBound:UC-137:SwitchRefundToBankTransferInput/MarkCardRefundFailedInput", "D-1606"),
        new("ModelWithoutEndpoint:SwitchRefundToBankTransferInput/MarkCardRefundFailedInput", "D-1606"),
        new("UseCaseNotListed:UC-132:ProcessRefundInput", "D-1606"),
        new("UseCaseNotListed:UC-136:RetryCardRefundInput", "D-1606"),
        new("UseCaseNotListed:UC-137:RetryCardRefundInput", "D-1606"),

        // D-1205: UC-02 binds CategoryQuery, not 07's ProductQuery.
        new("ModelNotBound:UC-02:ProductQuery", "D-1205"),
        new("UseCaseNotListed:UC-02:CategoryQuery", "D-1205"),
    ];

    /// <summary>
    /// What a test asserts empty: the problems that are no known difference, a <c>Stale</c> line for each known difference the check no longer reports, and a <c>NoBacklogItem</c>
    /// line for each known difference whose item is not a row of the backlog.
    /// </summary>
    public static List<string> Remaining(IEnumerable<string> problems, IReadOnlyList<KnownDifference> known)
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentNullException.ThrowIfNull(known);

        string[] found = [.. problems];
        List<string> remaining = [.. found.Where(p => known.All(k => k.Line != p))];
        remaining.AddRange(known.Where(k => !found.Contains(k.Line)).Select(k => $"Stale:{k.Line} ({k.Backlog}) - the difference is closed: delete the entry"));

        string backlog = BacklogText();
        remaining.AddRange(known.Select(k => k.Backlog).Distinct(StringComparer.Ordinal)
            .Where(id => !Regex.IsMatch(backlog, @"\|\s*" + Regex.Escape(id) + @"\s*\|", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))
            .Select(id => $"NoBacklogItem:{id}"));
        return remaining;
    }

    private static string BacklogText()
    {
        string docs = Path.Combine(RepositoryRoot.Find(), "Docs");
        IEnumerable<string> files = [Path.Combine(docs, "DEFERRED_BACKLOG.md"), .. Directory.GetFiles(Path.Combine(docs, "DEFERRED_BACKLOG.d"), "*.md")];
        return string.Join('\n', files.Select(f => File.ReadAllText(f)));
    }
}
