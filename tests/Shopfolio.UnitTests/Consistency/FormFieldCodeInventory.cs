namespace Shopfolio.UnitTests.Consistency;

/// <summary>A code check that names a field and an error code, that no line of <see cref="FormFieldEnforcementTable"/> holds, and the reason it is no check of a row of <c>04 7</c> (or cannot be a line of the row).</summary>
internal sealed record FormFieldCodeExemption(FormFieldCodeCall Call, string Reason);

/// <summary>
/// The code checks that <see cref="FormFieldCodeCalls"/> finds and that are no line of <see cref="FormFieldEnforcementTable"/> (D-1656), each with its reason. It is no list of known differences: the check of
/// a field of a row is a line of the table (the scan's findings that belong to a row are in <c>CodeCheckLines</c>); what stands here is a check of a flow that has no row, a page that only shows the
/// refusal of a module, a reader of a stored value, or a check that the row's own cells cannot hold without the comparison of <see cref="FormFieldEnforcement"/> refusing the line.
/// </summary>
internal static class FormFieldCodeInventory
{
    private const string Command = "the administrator command's call (CreateAdminAsync is called by CreateAdminCommand only): the first administrator is made from the command line, a flow with no form and no row of 04 7";
    private const string PageShowsRefusal = "the page puts the refusal code of the module (PasswordPolicy, the attribute) on the field, the fallback code is FieldRequired; the check is the module's, the page makes none";
    private const string OptionalRow = "a conditional requirement of an optional row (04 7.2.4.13 'İsteğe bağlı — seçilirse ... zorunludur', 7.2.4.6 'İsteğe bağlı'): a Required line on that row is RequirementContradictsCode";
    private const string ListInWords = "a closed list the row names in words without the 7.3.2.16 id (04 7.2.4.12, 7.2.4.17): a List line on the row is RuleMissesAttribute";

    public static IReadOnlyList<FormFieldCodeExemption> Exemptions { get; } =
    [
        new(new("Commands/CreateAdminCommand.cs", "Email", "FieldRequired"), Command),
        new(new("Commands/CreateAdminCommand.cs", "Email", "InvalidEmail"), Command),
        new(new("Commands/CreateAdminCommand.cs", "Password", "FieldRequired"), Command),
        new(new("Modules/Administration/AdministrationModule.cs", "Email", "FieldRequired"), Command),
        new(new("Modules/Administration/AdministrationModule.cs", "Email", "InvalidEmail"), Command),
        new(new("Modules/Administration/AdministrationModule.cs", "Name", "FieldRequired"), Command),
        new(new("Modules/Catalog/ProductSaves.cs", "FirstOptionValueId", "FieldRequired"), OptionalRow),
        new(new("Modules/Catalog/ProductSaves.cs", "FirstOptionValueId", "InvalidListValue"), OptionalRow),
        new(new("Modules/Catalog/ProductSaves.cs", "MeasurementUnit", "FieldRequired"), OptionalRow),
        new(new("Modules/Catalog/ProductSaves.cs", "NetQuantity", "FieldRequired"), OptionalRow),
        new(new("Modules/Catalog/ProductSaves.cs", "OriginCountryId", "InvalidListValue"), ListInWords),
        new(new("Modules/Catalog/ProductSaves.cs", "WithdrawalExemptionReason", "InvalidListValue"), ListInWords),
        new(new("Modules/CorporateContent/ContentReaders.cs", "MapUrl", "InvalidUrl"), "a reader of a stored value: a map link that is no link is not shown (MapLinkOf); the check of the input is the line of 7.2.16.x in ContentInputs.cs"),
        new(new("Modules/Settings/FirmSettings.cs", "DefaultVatRate", "InvalidPercentage"), "04 7.2.21.5 names P-8 and P-10 and no 7.3.2.9: a Percentage line on the row is RuleMissesAttribute"),
        new(new("Pages/Account/Profile.cshtml.cs", "Input.NewPassword", "PasswordTooWeak"), PageShowsRefusal),
        new(new("Pages/Panel/HomeAndMenu.cshtml.cs", "Input.", "TextTooLong"), "the page puts the refusal of FirmSettings on the menu name's field and passes the P-27 ceiling as the message's parameter (D-1673); the check is the line of 7.2.17.2 in FirmSettings.cs"),
        new(new("Pages/Panel/MyAccount.cshtml.cs", "Input.Name", "FieldRequired"), PageShowsRefusal),
        new(new("Pages/Panel/MyAccount.cshtml.cs", "Input.NewEmail", "FieldRequired"), PageShowsRefusal),
        new(new("Pages/Panel/MyAccount.cshtml.cs", "Input.NewPassword", "PasswordTooWeak"), PageShowsRefusal),
        new(new("Pages/Panel/Reset.cshtml.cs", "Input.NewPassword", "PasswordTooWeak"), PageShowsRefusal),
        new(new("Pages/Storefront/PasswordReset.cshtml.cs", "Input.NewPassword", "PasswordTooWeak"), PageShowsRefusal),
        new(new("Pages/Panel/PaymentMethods.cshtml.cs", "BankTransferIban", "FieldRequired"), "the page maps the rejections of PaymentMethodSettings to the field's messages (07 6.2.8); the check is the line of 7.2.20.2 in PaymentMethodSettings.cs"),
        new(new("Pages/Panel/PaymentMethods.cshtml.cs", "Label", "FilledFieldCleared"), "the message of the emptied IBAN (FieldMessageCatalog.Build) is a text, not a check; the check is the line of 7.2.20.2 in PaymentMethodSettings.cs"),
    ];

    public static IReadOnlyList<FormFieldCodeCall> Unlisted { get; } = [.. Exemptions.Select(e => e.Call)];
}
