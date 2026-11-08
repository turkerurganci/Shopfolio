namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The open surface of the Settings module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the company identity, the sales gate, the legal texts and the delivery provinces (05 3.3.10).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3). The installation has one company (02 3.1.1): there is no member that adds, selects, switches or deletes a
/// company, and none that chooses a language, a market or a currency.
/// </summary>
public interface ISettingsModule
{
    /// <summary>The company identity as the identity form shows it (T330; 02 3.1.2).</summary>
    Task<CompanyIdentity> GetCompanyIdentityAsync(CancellationToken ct);

    /// <summary>
    /// The fields the identity still waits for (02 3.1.3, 3.1.5 condition 1): read from the row on every call, never stored. A caller that decides
    /// the sales gate (T331) reads this and nothing else of the identity.
    /// </summary>
    Task<CompanyIdentityCompleteness> GetCompanyIdentityCompletenessAsync(CancellationToken ct);

    /// <summary>
    /// What a visitor reads under the "İletişim" heading (02 3.1.4): open to everyone, no administrator session is asked or taken.
    /// </summary>
    Task<LegalIdentity> GetLegalIdentityAsync(CancellationToken ct);

    /// <summary>
    /// The identity form's save (02 3.1.2, 3.1.3; 07 9.16.2): in the caller's open transaction, which the method joins and never opens. The row of the
    /// company settings is locked first (05 7.3.5; K-1088), the values are checked against the row as it is, and the row is written with the
    /// version the form carried (09 10.2 item 2). An empty required field does not stop the save - the sales gate stays closed - but a filled
    /// required field of the chosen type or of the three types' common set cannot be emptied, and a type once chosen cannot be cleared; changing the
    /// type is never refused. The refusals are results.
    /// </summary>
    Task<SettingsSaveResult> SaveCompanyIdentityAsync(CompanyIdentityInput input, int formVersion, CancellationToken ct);

    /// <summary>The return address section (02 3.1.7): the address when it is complete, and the section's version.</summary>
    Task<ReturnAddressSection> GetReturnAddressAsync(CancellationToken ct);

    /// <summary>
    /// The return address form's save (02 3.1.7; 09 10.3): in the caller's open transaction, row lock first, then the conditional update of this
    /// section's version only. The four required parts come together or not at all; once filled they cannot be emptied; the telephone is optional.
    /// A declaration copies <see cref="GetReturnAddressAsync"/>'s address as a value, so a later change never reaches a made declaration (T372).
    /// </summary>
    Task<SettingsSaveResult> SaveReturnAddressAsync(ReturnAddressInput input, int formVersion, CancellationToken ct);

    /// <summary>The shipping fee section (02 11.1 P-1, P-3, P-4): the fee, the two optional amounts and the section's version.</summary>
    Task<ShippingFeeSection> GetShippingFeeAsync(CancellationToken ct);

    /// <summary>
    /// The shipping fee form's save (T381; 07 UC-168): in the caller's open transaction, row lock first, then the conditional update of this section's version only, and
    /// the trail row with the old and new value of each changed field in the same transaction (02 10.3.1 (6)). A negative, over-limit or more-than-two-decimal amount and an
    /// empty fee are refusals; a save that changes nothing writes nothing.
    /// </summary>
    Task<SettingsSaveResult> SaveShippingFeeAsync(ShippingFeeInput input, int formVersion, CancellationToken ct);

    /// <summary>The periods section (02 11.1 P-5, P-7): the firm default of the shipping period, the bank transfer payment window and the section's version.</summary>
    Task<PeriodsSection> GetPeriodsAsync(CancellationToken ct);

    /// <summary>
    /// The periods form's save (T381; 07 UC-170): same transaction, lock and trail as <see cref="SaveShippingFeeAsync"/>. A shipping period over P-5's upper fence and a payment
    /// window outside P-7's lower or upper fence is a refusal that names the side and the fence value (02 4.1.5).
    /// </summary>
    Task<SettingsSaveResult> SavePeriodsAsync(PeriodsInput input, int formVersion, CancellationToken ct);

    /// <summary>The product defaults section (02 11.1 P-8, P-10): the download allowance, the VAT rate default and the section's version.</summary>
    Task<ProductDefaultsSection> GetProductDefaultsAsync(CancellationToken ct);

    /// <summary>
    /// The product defaults form's save (T381; 07 UC-171): same transaction, lock and trail. An allowance under P-8's lower fence and a rate outside 0 to under 100 with two decimals
    /// are refusals. The rate of an existing product does not change (a product carries its own rate).
    /// </summary>
    Task<SettingsSaveResult> SaveProductDefaultsAsync(ProductDefaultsInput input, int formVersion, CancellationToken ct);

    /// <summary>
    /// The decision of the sales gate and of the gate of the data-collecting entries (T331; 02 3.1.5): computed in one place from the database on every call and never
    /// stored (05 4.1.5). The storefront, the cart, the payment step, the order confirmation's transaction, the registration, the Google sign-in, the contact form and
    /// the panel's setup checklist all read this value. The method only reads and opens no transaction.
    /// </summary>
    Task<SalesGate> GetSalesGateAsync(CancellationToken ct);

    /// <summary>
    /// Closes sales temporarily (T331; 02 3.1.6; 07 9.16.3): one update against the value the screen showed (<paramref name="shownIsClosed"/>, no version, no row lock) and the
    /// trail row in the caller's transaction, by the administrator of the request. A changed or already closed switch writes nothing and is a result.
    /// </summary>
    Task<SalesSwitchResult> CloseSalesTemporarilyAsync(bool shownIsClosed, CancellationToken ct);

    /// <summary>
    /// Opens sales again (T331; 02 3.1.6; 07 9.16.4): switches the temporary closure off against the value the screen showed. The switch alone does not open sales - the result's
    /// gate says whether they are open or which conditions still hold them closed. Same transaction and trail as <see cref="CloseSalesTemporarilyAsync"/>.
    /// </summary>
    Task<SalesSwitchResult> ReopenSalesAsync(bool shownIsClosed, CancellationToken ct);

    /// <summary>
    /// The four legal texts as the panel lists them (T366; 02 3.33.1, 3.33.9): each with its last published number and the parts that are missing, by name. The privacy
    /// notice is missing its publication, its hosting location or the identity fields; the cookie policy its publication; the two generated texts their first version.
    /// </summary>
    Task<IReadOnlyList<LegalTextSummary>> GetLegalTextsAsync(CancellationToken ct);

    /// <summary>The draft of a firm-edited text (the privacy notice or the cookie policy); another type is an <see cref="ArgumentException"/> (02 3.33.1).</summary>
    Task<LegalTextDraft> GetLegalTextDraftAsync(LegalTextType type, CancellationToken ct);

    /// <summary>
    /// Saves the draft of a firm-edited text (T366; 02 3.33.2): in the caller's transaction, the record variant against the draft's version. The text cannot be emptied; the
    /// hosting location exists on the privacy notice only and cannot be emptied once filled. The text is taken as given: the closed-format cleaning of the HTML is the panel
    /// page's step before this call (09 15.3; T507).
    /// </summary>
    Task<SettingsSaveResult> SaveLegalTextDraftAsync(LegalTextType type, string text, string? hostingLocation, int formVersion, CancellationToken ct);

    /// <summary>
    /// Publishes the draft of a firm-edited text as the next version (T366; 02 3.33.2, 3.33.3): the number is the last plus one, the row is added with the identity placeholders
    /// kept (the reads fill them from the current identity - 04 9.19.11) and the trail row is written in the same transaction. The privacy notice is not published without its
    /// hosting location and every identity field of the chosen company type: each missing one is refused by its name.
    /// </summary>
    Task<LegalTextPublishResult> PublishLegalTextAsync(LegalTextType type, int formVersion, CancellationToken ct);

    /// <summary>The versions in force an order freezes at its confirmation (T366; 02 3.33.4).</summary>
    Task<LegalTextVersionIds> GetCurrentLegalTextVersionsAsync(CancellationToken ct);

    /// <summary>
    /// The confirmation step's check (T366; 02 3.33.4, 3.17.2): true when the form and the contract the customer was shown are still the last published versions of their
    /// types. When one has risen the order is not created and the customer confirms the current text; there is no re-consent window and no notice for a text that changed.
    /// </summary>
    Task<bool> AreShownLegalTextVersionsCurrentAsync(Guid shownPreInformationFormId, Guid shownDistanceSalesContractId, CancellationToken ct);

    /// <summary>A published version by its id; every version an order froze stays readable (02 3.33.3). <see langword="null"/> when there is no such version.</summary>
    Task<LegalTextVersionInfo?> GetLegalTextVersionAsync(Guid versionId, CancellationToken ct);

    /// <summary>The last published version of a type; <see langword="null"/> when there is none.</summary>
    Task<LegalTextVersionInfo?> GetLatestLegalTextVersionAsync(LegalTextType type, CancellationToken ct);

    /// <summary>
    /// Writes the first versions of the two generated texts and a new version when the shipped template changed (T366; 06 4.56, 09 11.4 item 5): the start-up step, a system
    /// event that writes no trail row. In the caller's transaction, row lock first.
    /// </summary>
    Task SynchronizeGeneratedLegalTextsAsync(CancellationToken ct);

    /// <summary>
    /// The closed list of the 81 provinces in plate-code order (T344; 02 3.14.3; 06 4.3): read only. An address form's list and the check of an address's province both read it.
    /// </summary>
    Task<IReadOnlyList<ProvinceInfo>> GetProvincesAsync(CancellationToken ct);

    /// <summary>The delivery provinces section (T350; 02 3.20.1): the selected provinces and the section's version. A plain read.</summary>
    Task<DeliveryProvincesSection> GetDeliveryProvincesAsync(CancellationToken ct);

    /// <summary>
    /// The delivery provinces form's save (T350; 02 3.20.1; 07 UC-169): in the caller's open transaction, row lock first, then the conditional update of this section's version and the
    /// province rows written and deleted in the same transaction, and the trail row with the old and new selection (plate codes) - 02 10.3.1 (6). A province that is not on the list of 81 is a
    /// refusal; no selection at all is accepted; a save that changes nothing writes nothing. The two generated texts are regenerated afterwards, as the other sections that feed them do.
    /// </summary>
    Task<SettingsSaveResult> SaveDeliveryProvincesAsync(IReadOnlyCollection<Guid> provinceIds, int formVersion, CancellationToken ct);

    /// <summary>
    /// The delivery area (T350; 02 3.20.1, 6.2.3): the provinces the company delivers to and the ones it does not. The storefront shows its restriction line before the address step, the payment
    /// step refuses a shipping address outside it and the order confirmation compares it again. A plain read; it opens no transaction.
    /// </summary>
    Task<DeliveryArea> GetDeliveryAreaAsync(CancellationToken ct);
}
