// Deliberate violation sample of the legal text scan (11 T366 criteria 3, 5, 6, 10): a consent record, a banner, an attachment of the generated text and a row made by hand.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class ConsentFileAndRowCreation(AppDbContext db)
{
    public DbSet<PrivacyNoticeConsent> Consents => db.Set<PrivacyNoticeConsent>();
    public bool AcceptedPrivacyNotice { get; set; }
    public DateTimeOffset? CookiePolicySeenAt { get; set; }
    public string Banner() => "cookie_banner";
    public string Explicit() => "explicit consent: açık rıza";
    public string Attach(Order order) => order.PreInformationFormText + ".pdf";
    public object Part() => new MultipartFormDataContent();
    public LegalTextVersion ByHand() => LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 7, DateTimeOffset.UtcNow, "x", DateTimeOffset.UtcNow);
    public LegalTextVersion ByConstructor() => new LegalTextVersion { VersionNumber = 7 };
    public LegalTextVersion Draft() => LegalTextVersion
        .CreateDraft(LegalTextType.CookiePolicy, "x", DateTimeOffset.UtcNow);

    // Not findings: a comment that says AcceptedPrivacyNotice and .pdf, a read of the versions, the checkbox record of the form and the contract.
    public Task<LegalTextVersionInfo?> Read(ISettingsModule settings, CancellationToken ct) => settings.GetLatestLegalTextVersionAsync(LegalTextType.PrivacyNotice, ct);
    public object Checkbox() => new CheckboxAcceptance();
}
