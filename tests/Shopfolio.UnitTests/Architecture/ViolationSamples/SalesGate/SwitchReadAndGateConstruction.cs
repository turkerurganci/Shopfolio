// Deliberate violation sample of the sales gate scan (11 T331 criterion 8): a surface that decides from the switch and builds a gate of its own.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class SwitchReadAndGateConstruction(AppDbContext db)
{
    public Task<bool> ReadsTheProperty(CancellationToken ct) => db.CompanySettings.AsNoTracking().Select(s => s.IsSalesTemporarilyClosed).SingleAsync(ct);
    public string NameOf() => nameof(CompanySettings.IsSalesTemporarilyClosed);
    public string AsAString() => "IsSalesTemporarilyClosed";
    public string CamelCased() => "isSalesTemporarilyClosed";
    public Task<bool> RawColumn(CancellationToken ct) => db.Database.SqlQuery<bool>($"SELECT is_sales_temporarily_closed AS \"Value\" FROM company_settings").SingleAsync(ct);
    public SalesGate Built() => new SalesGate([], []);
    public SalesGate BuiltQualified() => new Shopfolio.Modules.Settings.Contracts.SalesGate(
        [], []);
    public SalesGate BuiltTargetTyped()
    {
        SalesGate gate = new([], []);
        return gate;
    }

    // Not findings: a comment that says IsSalesTemporarilyClosed and new SalesGate(, the hidden field of the shown value, a type that starts with the name, and a read of the gate.
    public string Hidden() => nameof(OriginalIsSalesTemporarilyClosed);
    public object Facts() => new SalesGateFacts([], true, false, true, true, true);
    public Task<SalesGate> Read(ISettingsModule settings, CancellationToken ct) => settings.GetSalesGateAsync(ct);
}
