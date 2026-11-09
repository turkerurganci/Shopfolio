using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Ordering.Documents;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The confirmation step's content and check (T356; 02 3.24; 04 5.13.9): reads the last published form and contract, writes the summary and fills the customer's tokens from the summary, and
/// checks the versions shown and the boxes ticked. It writes nothing and opens no transaction (the order is born in T348's transaction); its only dependency is the Settings module's open face
/// (09 6 item 2). The module's own constructor is not touched: the step is a scoped service of its own.
/// </summary>
internal sealed class ConfirmationStep : IConfirmationStep
{
    private readonly ISettingsModule _settings;

    public ConfirmationStep(ISettingsModule settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
    }

    public async Task<ConfirmationStepView> GetViewAsync(ConfirmationSummary summary, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(summary);
        IReadOnlyList<ConfirmationBox> needed = ConfirmationStepRules.NeededBoxes(summary.Lines);
        LegalTextVersionInfo form = await _settings.GetLatestLegalTextVersionAsync(LegalTextType.PreInformationForm, ct)
            ?? throw new InvalidOperationException("The Pre-Information Form has no published version; the sales gate keeps the step closed (02 3.1.5).");
        LegalTextVersionInfo contract = await _settings.GetLatestLegalTextVersionAsync(LegalTextType.DistanceSalesContract, ct)
            ?? throw new InvalidOperationException("The Distance Sales Contract has no published version; the sales gate keeps the step closed (02 3.1.5).");
        IReadOnlyDictionary<Guid, string> provinces = (await _settings.GetProvincesAsync(ct)).ToDictionary(p => p.Id, p => p.Name);

        // The same two stages as the order's: the frozen values first, then the customer's tokens - here from the summary, there from the confirmation's parties.
        Dictionary<string, string?> values = new(OrderDocuments.FrozenValues(OrderDocuments.OrderNumberBeforeConfirmation, summary.Total, summary.ShippingFee, summary.ShippingPeriod), StringComparer.Ordinal);
        foreach ((string name, string? value) in OrderDocuments.PersonalValues(summary.ContactEmail, summary.Shipping, summary.Billing, provinces))
        {
            values[name] = value;
        }

        string summaryText = ConfirmationSummaryText.Render(summary);
        return new ConfirmationStepView(
            summaryText,
            summaryText + "\n\n" + PlaceholderFiller.Fill(form.Text, values, requireAll: false),
            PlaceholderFiller.Fill(contract.Text, values, requireAll: false),
            form.Id,
            contract.Id,
            ProcessGuide.Text,
            ConfirmationStepRules.WithdrawalSummary(summary.Lines),
            ConfirmationStepRules.Offers(summary.Lines, summary.PaymentMethod),
            string.IsNullOrWhiteSpace(summary.ContactEmail) ? null : ConfirmationSummaryText.EmailLine(summary.ContactEmail),
            ConfirmationStepRules.PaymentObligationNotice,
            ConfirmationStepRules.ConfirmButtonText);
    }

    public async Task<ConfirmationStepCheck> CheckAsync(ConfirmationStepSubmission submission, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(submission);

        // An order with no line is a programming error and throws before anything else is read.
        _ = ConfirmationStepRules.NeededBoxes(submission.Lines);

        // The risen version is looked at first: the customer confirms the current text, so the ticks of the old one are not looked at (02 3.17.2; K-591).
        if (!await _settings.AreShownLegalTextVersionsCurrentAsync(submission.ShownPreInformationFormId, submission.ShownDistanceSalesContractId, ct))
        {
            return new ConfirmationStepCheck(true, []);
        }

        IReadOnlyList<ConfirmationBox> missing = ConfirmationStepRules.MissingBoxes(submission.Lines, submission.Ticked);
        return new ConfirmationStepCheck(false, missing);
    }
}
