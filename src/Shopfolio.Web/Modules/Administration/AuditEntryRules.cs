using System.Collections.Frozen;
using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The rules an audit entry must meet before it is written (09 9.2 items 1, 4; 02 10.3.1; 06 4.63). A break is a programming error and the caller gets an
/// <see cref="ArgumentException"/>: the trail is not the place to find out that a customer's address was written into it.
/// <list type="bullet">
/// <item>The operation is a defined code and the target record has the kind the operation owns; the kind and the id are given together, and an export has neither.</item>
/// <item>A company, branding or settings change carries its changed fields with the old and the new value.</item>
/// <item>An order operation and the deletion of a customer's account carry no personal data of the customer in any slot of the details: no field or value is named
/// after a name, an address, a phone, an email, an IBAN or a note; every value is a code, a date, a count, an amount or an id by the shape of its name; no value looks
/// like an email address or an IBAN. The display name of an order row is the order number; the deletion of an account has none (it names the operation and its date,
/// not the account). The correction of an address, of a contact email, an internal note and the deletion of an account carry no old-new pair at all.</item>
/// </list>
/// </summary>
internal static partial class AuditEntryRules
{
    /// <summary>The most changed fields or values a row holds (a settings save changes a handful, never hundreds).</summary>
    public const int MaxItems = 100;

    public const int MaxNameLength = 100;

    public const int MaxValueLength = 2000;

    /// <summary>The longest code a protected row carries as a value.</summary>
    public const int MaxCodeLength = 64;

    /// <summary>The operations that must not carry an old-new pair (09 9.2 item 4): the old and the new value would be the customer's data or a note's text.</summary>
    private static readonly FrozenSet<AuditOperation> NoPairOperations = new HashSet<AuditOperation>
    {
        AuditOperation.CorrectAddress,
        AuditOperation.CorrectContactEmail,
        AuditOperation.SaveInternalNote,
        AuditOperation.DeleteCustomerAccount,
    }.ToFrozenSet();

    /// <summary>The operations that carry the changed fields of a firm setting with their old and new value (02 10.3.1 (1), (6)).</summary>
    private static readonly FrozenSet<AuditOperation> PairRequiredOperations = new HashSet<AuditOperation>
    {
        AuditOperation.UpdateCompany,
        AuditOperation.UpdateBranding,
        AuditOperation.UpdateSettings,
    }.ToFrozenSet();

    /// <summary>The field and value names that stand for a customer's personal data, compared by their letters alone and in lower case.</summary>
    private static readonly FrozenSet<string> PersonalNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "name", "fullname", "firstname", "lastname", "surname", "customername", "recipientname", "buyername", "contactname", "accountholder", "accountholdername",
        "address", "shippingaddress", "billingaddress", "deliveryaddress", "invoiceaddress", "street", "addressline", "addressdetail", "neighborhood", "district",
        "phone", "phonenumber", "telephone", "mobile", "mobilephone", "deliveryphone",
        "email", "emailaddress", "contactemail", "customeremail", "oldemail", "newemail",
        "iban", "ibannumber", "bankaccount",
        "note", "notetext", "internalnote", "text", "notebody", "message",
    }.ToFrozenSet(StringComparer.Ordinal);

    /// <summary>The closed shapes a value of an order row or of the deletion of an account takes: a code, a date, a count or an amount, an id - never a customer's text.</summary>
    private enum ValueShape
    {
        Code,
        Date,
        Count,
        Amount,
        Id,
    }

    /// <summary>
    /// The closed list of the field and value names an order row and the deletion of an account may carry (letters only, lower case), each with the one shape its value
    /// takes. A name that can carry free text (a carrier's name, a tracking number, an item's name) is not on the list. The audit trail interface is the one place this
    /// list is kept (09 9.2 item 4).
    /// </summary>
    private static readonly FrozenDictionary<string, ValueShape> CustomerSafeNames = new Dictionary<string, ValueShape>(StringComparer.Ordinal)
    {
        ["field"] = ValueShape.Code,
        ["operation"] = ValueShape.Code,
        ["reason"] = ValueShape.Code,
        ["reasoncode"] = ValueShape.Code,
        ["correctionreason"] = ValueShape.Code,
        ["transitioncode"] = ValueShape.Code,
        ["fromstatus"] = ValueShape.Code,
        ["tostatus"] = ValueShape.Code,
        ["channel"] = ValueShape.Code,
        ["count"] = ValueShape.Count,
        ["quantity"] = ValueShape.Count,
        ["oldquantity"] = ValueShape.Count,
        ["newquantity"] = ValueShape.Count,
        ["amount"] = ValueShape.Amount,
        ["date"] = ValueShape.Date,
        ["requestdate"] = ValueShape.Date,
        ["requestedat"] = ValueShape.Date,
        ["ibanrequestedat"] = ValueShape.Date,
        ["deliveredat"] = ValueShape.Date,
        ["shippedat"] = ValueShape.Date,
        ["arrivalat"] = ValueShape.Date,
        ["arrivaldate"] = ValueShape.Date,
        ["returnarrivaldate"] = ValueShape.Date,
        ["deliverydate"] = ValueShape.Date,
        ["itemid"] = ValueShape.Id,
        ["refundid"] = ValueShape.Id,
        ["returnid"] = ValueShape.Id,
        ["paymentid"] = ValueShape.Id,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>The fixed kind of the target record of an operation (09 9.2 item 3); an operation that is not here takes any kind (a publication takes the kind of the record, 06 5.2.3).</summary>
    public static IReadOnlyList<string>? TargetTypesOf(AuditOperation operation) => operation switch
    {
        AuditOperation.UpdateCompany => ["Company"],
        AuditOperation.UpdateBranding => ["Branding"],
        AuditOperation.SendAdminInvitation or AuditOperation.WithdrawAdminInvitation => ["AdminInvitation"],
        AuditOperation.CreateAdminAccount or AuditOperation.RemoveAdminAccount or AuditOperation.ChangeAdminEmail => ["Admin"],
        AuditOperation.PublishLegalText => ["LegalTextVersion"],
        AuditOperation.UpdateSettings => ["CompanySettings", "Province", "Branding", "Product"],
        AuditOperation.ExportOrders or AuditOperation.ExportCustomers or AuditOperation.ExportContactRequests => [],
        AuditOperation.DeleteCustomerAccount => ["Customer"],
        AuditOperation.ChangePublishStatus => null,
        _ => AuditOperationText.AreaOf(operation) == AuditArea.Order ? ["Order"] : null,
    };

    /// <summary>Whether the operation's row is held to the rule that it carries no customer data (the order operations and the deletion of an account).</summary>
    public static bool ProtectsCustomerData(AuditOperation operation)
        => AuditOperationText.AreaOf(operation) == AuditArea.Order || operation == AuditOperation.DeleteCustomerAccount;

    public static void Validate(AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (!Enum.IsDefined(entry.Operation))
        {
            throw new ArgumentException("The operation is not an audit operation code.", nameof(entry));
        }

        ValidateTarget(entry);
        ValidateDetails(entry);
    }

    private static void ValidateTarget(AuditEntry entry)
    {
        bool hasType = !string.IsNullOrWhiteSpace(entry.TargetRecordType);
        if (hasType != entry.TargetRecordId.HasValue)
        {
            throw new ArgumentException("The target record's kind and id are given together or not at all (06 4.63).", nameof(entry));
        }

        if (entry.TargetRecordId == Guid.Empty)
        {
            throw new ArgumentException("The target record's id is empty.", nameof(entry));
        }

        if (hasType && entry.TargetRecordType!.Length > MaxNameLength) // non-null: hasType means the type is not null or white space
        {
            throw new ArgumentException("The target record's kind is too long.", nameof(entry));
        }

        IReadOnlyList<string>? allowed = TargetTypesOf(entry.Operation);
        if (allowed is null)
        {
            if (!hasType)
            {
                throw new ArgumentException($"{entry.Operation} has a target record (09 9.2 item 3).", nameof(entry));
            }

            return;
        }

        if (allowed.Count == 0)
        {
            if (hasType)
            {
                throw new ArgumentException($"{entry.Operation} has no target record (06 4.63).", nameof(entry));
            }

            return;
        }

        if (!hasType || !allowed.Contains(entry.TargetRecordType!, StringComparer.Ordinal)) // non-null: read only when hasType, which means the type is not null or white space
        {
            throw new ArgumentException($"{entry.Operation} takes the target kind {string.Join(" or ", allowed)} (09 9.2 item 3).", nameof(entry));
        }
    }

    private static void ValidateDetails(AuditEntry entry)
    {
        AuditDetails? details = entry.Details;
        bool needsPairs = PairRequiredOperations.Contains(entry.Operation);
        if (details is null)
        {
            if (needsPairs)
            {
                throw new ArgumentException($"{entry.Operation} carries the changed fields with their old and new value (02 10.3.1).", nameof(entry));
            }

            return;
        }

        if (details.Changes.Count > MaxItems || details.Values.Count > MaxItems)
        {
            throw new ArgumentException("The details hold too many values.", nameof(entry));
        }

        if (needsPairs && details.Changes.Count == 0)
        {
            throw new ArgumentException($"{entry.Operation} carries the changed fields with their old and new value (02 10.3.1).", nameof(entry));
        }

        CheckLength(details.DisplayName, MaxValueLength);
        foreach (AuditValueChange change in details.Changes)
        {
            RequireName(change.Field);
            CheckLength(change.OldValue, MaxValueLength);
            CheckLength(change.NewValue, MaxValueLength);
        }

        foreach (AuditValue value in details.Values)
        {
            RequireName(value.Name);
            CheckLength(value.Value, MaxValueLength);
        }

        if (!ProtectsCustomerData(entry.Operation))
        {
            return;
        }

        if (NoPairOperations.Contains(entry.Operation) && details.Changes.Count > 0)
        {
            throw new ArgumentException($"{entry.Operation} carries no old and new value (09 9.2 item 4).", nameof(entry));
        }

        CheckDisplayName(entry.Operation, details.DisplayName);

        foreach (AuditValueChange change in details.Changes)
        {
            ValueShape shape = RefuseName(change.Field);
            CheckShape(entry.Operation, change.Field, shape, change.OldValue);
            CheckShape(entry.Operation, change.Field, shape, change.NewValue);
        }

        foreach (AuditValue value in details.Values)
        {
            CheckShape(entry.Operation, value.Name, RefuseName(value.Name), value.Value);
        }

        foreach (string? text in Texts(details))
        {
            if (text is not null && (text.Contains('@', StringComparison.Ordinal) || Iban().IsMatch(text)))
            {
                throw new ArgumentException($"A value of {entry.Operation} looks like an email address or an IBAN: the customer's data is not written (09 9.2 item 4).", nameof(entry));
            }
        }
    }

    private static IEnumerable<string?> Texts(AuditDetails details)
    {
        yield return details.DisplayName;
        foreach (AuditValueChange change in details.Changes)
        {
            yield return change.OldValue;
            yield return change.NewValue;
        }

        foreach (AuditValue value in details.Values)
        {
            yield return value.Value;
        }
    }

    private static void RequireName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength)
        {
            throw new ArgumentException("A field or value name of the details is empty or too long.");
        }
    }

    private static void CheckLength(string? text, int max)
    {
        if (text is not null && text.Length > max)
        {
            throw new ArgumentException("A value of the details is too long.");
        }
    }

    // The display name of an order row is the order number and nothing else; the deletion of an account names the operation and its date, not the account (02 10.3.1 (3), (8)).
    private static void CheckDisplayName(AuditOperation operation, string? displayName)
    {
        if (displayName is null)
        {
            return;
        }

        if (operation == AuditOperation.DeleteCustomerAccount)
        {
            throw new ArgumentException("The deletion of an account names the operation and its date, not the account: it has no display name (06 4.63; 02 10.3.1 (8)).");
        }

        if (!OrderNumber().IsMatch(displayName))
        {
            throw new ArgumentException($"The display name of {operation} is the order number only (09 9.2 item 4).");
        }
    }

    private static void CheckShape(AuditOperation operation, string name, ValueShape shape, string? value)
    {
        if (value is null)
        {
            return;
        }

        bool fits = shape switch
        {
            ValueShape.Code => value.Length <= MaxCodeLength && CodeText().IsMatch(value),
            ValueShape.Date => DateText().IsMatch(value),
            ValueShape.Count => CountText().IsMatch(value),
            ValueShape.Amount => AmountText().IsMatch(value),
            ValueShape.Id => Guid.TryParseExact(value, "D", out _),
            _ => false,
        };
        if (!fits)
        {
            throw new ArgumentException($"The value of '{name}' of {operation} is not a {shape.ToString().ToLowerInvariant()}: the customer's text is not written (09 9.2 item 4).");
        }
    }

    private static ValueShape RefuseName(string name)
    {
        string key = new([.. name.Where(char.IsLetter).Select(c => char.ToLower(c, CultureInfo.InvariantCulture))]);
        if (PersonalNames.Contains(key))
        {
            throw new ArgumentException($"The name '{name}' stands for a customer's personal data: it is not written into the trail (09 9.2 item 4).");
        }

        // The rows that protect the customer take the names of a closed list: a free name could carry the customer's text under an innocent field.
        if (!CustomerSafeNames.TryGetValue(key, out ValueShape shape))
        {
            throw new ArgumentException($"The name '{name}' is not on the closed list of the names an order row may carry (09 9.2 item 4); a new name is added to the list with its reason.");
        }

        return shape;
    }

    // Order number: the year, a hyphen and the random block (02 3.22.1, P-19).
    [GeneratedRegex(@"^\d{4}-[A-Za-z0-9]{4,12}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex OrderNumber();

    // A code: ASCII letters and digits, starting with a letter, parts joined by . _ or - - no space, no Turkish letter, so a sentence or a full name is not one.
    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9]*(?:[._\-][A-Za-z0-9]+)*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CodeText();

    // A date or a moment in ISO 8601: a phone number or a house number is not one.
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}(?:[T ]\d{2}:\d{2}(?::\d{2}(?:\.\d{1,7})?)?(?:Z|[+\-]\d{2}:\d{2})?)?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DateText();

    // A count has at most six digits, so an eleven-digit phone number is not one.
    [GeneratedRegex(@"^\d{1,6}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CountText();

    [GeneratedRegex(@"^\d{1,9}(?:\.\d{1,2})?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AmountText();

    // A Turkish IBAN is TR, two check digits and twenty-two digits (the only account the product refunds to); spaces and hyphens inside are the way it is written by hand.
    // A cargo tracking number is not caught: it has no TR prefix followed by twenty-four digits.
    [GeneratedRegex(@"(?<![A-Za-z0-9])TR\d{2}(?:[ \-]?\d){22}(?!\d)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Iban();
}
