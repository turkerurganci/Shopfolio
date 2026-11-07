using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.UnitTests.Administration;

/// <summary>
/// The rules of an audit entry (T380 KK1-KK3, KK7; 09 9.2 items 1, 3, 4; 02 10.3.1, 10.3.3): the eight areas are accepted, the changed fields of a setting carry the old
/// and the new value, the rows of an order carry no personal data of the customer in any of the places a value is written, and the operations of the system are
/// not codes of the trail.
/// </summary>
public sealed class AuditEntryRulesTests
{
    private static readonly Guid Target = Guid.Parse("018f0000-0000-7000-8000-000000000001");

    private static readonly string[] EmailAndIbanValues =
    [
        "ayse@example.com",
        "TR33 0006 1005 1978 6457 8413 26",
        "TR330006100519786457841326",
        "tr33-0006-1005-1978-6457-8413-26",
    ];

    // ---- KK1: the eight areas ----

    public static TheoryData<AuditArea, AuditOperation> OneOperationOfEveryArea()
    {
        return new TheoryData<AuditArea, AuditOperation>
        {
            { AuditArea.CompanyAndBranding, AuditOperation.UpdateCompany },
            { AuditArea.CompanyAndBranding, AuditOperation.UpdateBranding },
            { AuditArea.Publication, AuditOperation.ChangePublishStatus },
            { AuditArea.Order, AuditOperation.MarkShipped },
            { AuditArea.AdminAccounts, AuditOperation.SendAdminInvitation },
            { AuditArea.LegalTextVersion, AuditOperation.PublishLegalText },
            { AuditArea.Settings, AuditOperation.UpdateSettings },
            { AuditArea.Export, AuditOperation.ExportOrders },
            { AuditArea.CustomerDeletion, AuditOperation.DeleteCustomerAccount },
        };
    }

    [Theory]
    [MemberData(nameof(OneOperationOfEveryArea))]
    public void AnEntryOfEachOfTheEightAreas_IsAccepted_AndTheAreaComesFromTheOperation(AuditArea area, AuditOperation operation)
    {
        AuditEntry entry = ValidEntry(operation);

        AuditEntryRules.Validate(entry);

        Assert.Equal(area, AuditOperationText.AreaOf(operation));
    }

    [Fact]
    public void TheEightAreas_AreEightAndEveryOperationBelongsToOne()
    {
        Assert.Equal(8, Enum.GetValues<AuditArea>().Length);
        Assert.Equal(Enum.GetValues<AuditArea>().Order(), Enum.GetValues<AuditOperation>().Select(AuditOperationText.AreaOf).Distinct().Order());
        Assert.Equal(43, Enum.GetValues<AuditOperation>().Length);
    }

    [Fact]
    public void EveryOperation_HasAValidEntry_ThatTheRulesAccept()
    {
        foreach (AuditOperation operation in Enum.GetValues<AuditOperation>())
        {
            AuditEntryRules.Validate(ValidEntry(operation));
        }
    }

    // ---- KK2: the old and the new value ----

    [Theory]
    [InlineData(AuditOperation.UpdateCompany)]
    [InlineData(AuditOperation.UpdateBranding)]
    [InlineData(AuditOperation.UpdateSettings)]
    public void ACompanyBrandingOrSettingsChange_WithoutTheChangedFields_IsRefused(AuditOperation operation)
    {
        string type = AuditEntryRules.TargetTypesOf(operation)![0];

        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(operation, type, Target, null)));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(operation, type, Target, new AuditDetails("Ayar"))));
    }

    [Fact]
    public void ASettingsChange_KeepsTheFirmsOwnIbanAndReturnAddress_AsOldAndNewValues()
    {
        AuditDetails details = new(
            "Havale bilgisi",
            [
                new AuditValueChange("Iban", "TR33 0006 1005 1978 6457 8413 26", "TR12 0001 2009 4520 0058 0000 01"),
                new AuditValueChange("ReturnAddress", "Eski mah. 1. sk.", "Yeni mah. 2. sk."),
                new AuditValueChange("MaxPreparationDays", "3", "5"),
            ]);

        AuditEntryRules.Validate(new AuditEntry(AuditOperation.UpdateSettings, "CompanySettings", Target, details));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AChangedFieldWithoutAName_IsRefused(string field)
    {
        AuditDetails details = new(changes: [new AuditValueChange(field, "1", "2")]);

        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.UpdateSettings, "CompanySettings", Target, details)));
    }

    [Fact]
    public void TooManyValues_OrTooLongAValue_IsRefused()
    {
        AuditValueChange[] many = [.. Enumerable.Range(0, AuditEntryRules.MaxItems + 1).Select(i => new AuditValueChange("F" + i, "1", "2"))];
        AuditValueChange tooLong = new("F", new string('x', AuditEntryRules.MaxValueLength + 1), "2");

        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.UpdateSettings, "CompanySettings", Target, new AuditDetails(changes: many))));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.UpdateSettings, "CompanySettings", Target, new AuditDetails(changes: [tooLong]))));
    }

    // ---- KK3: no personal data of the customer ----

    [Theory]
    [InlineData("Name")]
    [InlineData("name")]
    [InlineData("CustomerName")]
    [InlineData("Customer_Name")]
    [InlineData("shipping-address")]
    [InlineData("BillingAddress")]
    [InlineData("Phone")]
    [InlineData("DeliveryPhone")]
    [InlineData("Email")]
    [InlineData("ContactEmail")]
    [InlineData("Iban")]
    [InlineData("IBAN")]
    [InlineData("Note")]
    [InlineData("InternalNote")]
    [InlineData("NoteText")]
    public void AnOrderRow_RefusesAValueNamedAfterThePersonalDataOfTheCustomer(string name)
    {
        foreach (AuditOperation operation in OrderOperations())
        {
            AuditDetails asValue = new("Sipariş 2026ABCD", values: [new AuditValue(name, "x")]);
            Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(operation, "Order", Target, asValue)));
        }
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Address")]
    [InlineData("Phone")]
    [InlineData("Email")]
    [InlineData("Iban")]
    [InlineData("Note")]
    public void AnOrderRow_RefusesAChangedFieldNamedAfterThePersonalData_ForTheOperationsThatMayHoldPairs(string name)
    {
        AuditDetails details = new(changes: [new AuditValueChange(name, "x", "y")]);

        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.CorrectShippingInfo, "Order", Target, details)));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.CorrectDeliveryDate, "Order", Target, details)));
    }

    [Fact]
    public void AnOrderRow_RefusesAnEmailAndAnIbanValue_InEveryPlaceAValueIsWritten()
    {
        foreach (AuditOperation operation in OrderOperations().Append(AuditOperation.DeleteCustomerAccount))
        {
            string type = operation == AuditOperation.DeleteCustomerAccount ? "Customer" : "Order";
            foreach (string value in EmailAndIbanValues)
            {
                AuditDetails[] places =
                [
                    new(displayName: value),
                    new("x", values: [new AuditValue("Reason", value)]),
                    new("x", changes: [new AuditValueChange("TrackingNumber", value, "1")]),
                    new("x", changes: [new AuditValueChange("TrackingNumber", "1", value)]),
                ];
                foreach (AuditDetails details in places)
                {
                    Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(operation, type, Target, details)));
                }
            }
        }
    }

    [Theory]
    [InlineData(AuditOperation.CorrectAddress)]
    [InlineData(AuditOperation.CorrectContactEmail)]
    [InlineData(AuditOperation.SaveInternalNote)]
    public void TheCorrectionOfAnAddressOrAnEmailAndTheNote_CarryNoOldAndNewPair(AuditOperation operation)
    {
        AuditDetails pair = new("2026ABCD", changes: [new AuditValueChange("Field", "a", "b")]);

        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(operation, "Order", Target, pair)));

        // The row names the order, the field and the operation only.
        AuditEntryRules.Validate(new AuditEntry(operation, "Order", Target, new AuditDetails("2026ABCD", values: [new AuditValue("Field", "ShippingAddress")])));
    }

    [Fact]
    public void TheDeletionOfAnAccount_CarriesNoPair_AndNamesTheOperationAndItsDate()
    {
        AuditDetails pair = new(changes: [new AuditValueChange("Email", "a", "b")]);

        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.DeleteCustomerAccount, "Customer", Target, pair)));
        AuditEntryRules.Validate(new AuditEntry(AuditOperation.DeleteCustomerAccount, "Customer", Target, new AuditDetails(values: [new AuditValue("RequestDate", "2026-11-05")])));
    }

    [Fact]
    public void TheIbanRequestRow_CarriesTheDateOnly()
    {
        AuditEntryRules.Validate(new AuditEntry(AuditOperation.ReceiveReturnWithIbanRequest, "Order", Target, new AuditDetails("2026ABCD", values: [new AuditValue("IbanRequestedAt", "2026-11-05")])));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(
            new AuditEntry(AuditOperation.ReceiveReturnWithIbanRequest, "Order", Target, new AuditDetails("2026ABCD", values: [new AuditValue("Iban", "TR330006100519786457841326")]))));
    }

    [Fact]
    public void ATrackingNumberAndADate_AreNotTakenForAnIbanOrAnEmail()
    {
        AuditDetails details = new(
            "2026ABCD",
            changes: [new AuditValueChange("TrackingNumber", "YK12345678901", "TR12345678901234"), new AuditValueChange("DeliveredAt", "2026-11-04", "2026-11-05")]);

        AuditEntryRules.Validate(new AuditEntry(AuditOperation.CorrectShippingInfo, "Order", Target, details));
    }

    // ---- The target ----

    [Fact]
    public void TheTargetKindAndId_AreGivenTogether_AndAnExportHasNone()
    {
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.MarkShipped, "Order", null, null)));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.MarkShipped, null, Target, null)));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.MarkShipped, null, null, null)));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.MarkShipped, "Order", Guid.Empty, null)));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.ExportOrders, "Order", Target, null)));
        AuditEntryRules.Validate(new AuditEntry(AuditOperation.ExportOrders, null, null, null));
    }

    [Fact]
    public void TheTargetKind_IsTheOneTheOperationOwns()
    {
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.MarkShipped, "Product", Target, null)));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.RemoveAdminAccount, "Order", Target, null)));
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry(AuditOperation.WithdrawAdminInvitation, "Admin", Target, null)));
        AuditEntryRules.Validate(new AuditEntry(AuditOperation.ChangePublishStatus, "Product", Target, null));
        AuditEntryRules.Validate(new AuditEntry(AuditOperation.ChangePublishStatus, "CorporateContent", Target, null));
        AuditEntryRules.Validate(new AuditEntry(AuditOperation.UpdateSettings, "Province", Target, new AuditDetails(changes: [new AuditValueChange("Enabled", "false", "true")])));
    }

    [Fact]
    public void AnOperationThatIsNotACode_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => AuditEntryRules.Validate(new AuditEntry((AuditOperation)9999, "Order", Target, null)));
        Assert.Throws<ArgumentNullException>(() => AuditEntryRules.Validate(null!));
    }

    // ---- KK7: no system event ----

    [Fact]
    public void TheCodes_NameNoSystemEvent_TheDisposalNoLoginNoUndeliveredEmailAndNoInstallationCommand()
    {
        string[] systemWords = ["Login", "SignIn", "Disposal", "Dispose", "Purge", "Bounce", "NotDelivered", "Undelivered", "Install", "Migrate", "Seed", "Heartbeat", "Backup", "Outage", "Job", "Schedule", "Retention", "Reset", "Unlock"];

        foreach (string code in Enum.GetNames<AuditOperation>())
        {
            Assert.DoesNotContain(systemWords, word => code.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        // The one e-mail code is the firm's resending of a mail marked "not delivered", which is an administrator's operation (02 10.3.1 (3)).
        Assert.Equal(["ResendEmail"], Enum.GetNames<AuditOperation>().Where(c => c.Contains("Email", StringComparison.Ordinal) && c.Contains("Resend", StringComparison.Ordinal)));
    }

    private static IEnumerable<AuditOperation> OrderOperations()
        => Enum.GetValues<AuditOperation>().Where(o => AuditOperationText.AreaOf(o) == AuditArea.Order);

    /// <summary>An entry that meets every rule of its operation.</summary>
    public static AuditEntry ValidEntry(AuditOperation operation)
    {
        IReadOnlyList<string>? kinds = AuditEntryRules.TargetTypesOf(operation);
        string? type = kinds is null ? "Product" : kinds.Count == 0 ? null : kinds[0];
        Guid? id = type is null ? null : Target;
        AuditDetails? details = operation is AuditOperation.UpdateCompany or AuditOperation.UpdateBranding or AuditOperation.UpdateSettings
            ? new AuditDetails("Ayar", [new AuditValueChange("Field", "1", "2")])
            : null;
        return new AuditEntry(operation, type, id, details);
    }
}
