using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The conditions of the sales gate, each by its name (02 3.1.5, 3.33.9, 6.9.10; 10 4.1 ÖK-8…ÖK-11): the panel and the refusals say which one is not met. The four
/// conditions of 02 3.1.5 are the identity, the payment method, the temporary closure and the four legal texts; the fourth is named by the part that is missing
/// (the privacy notice, the cookie policy, the return address that feeds the two generated texts), so the panel says which text waits.
/// </summary>
public enum SalesGateCondition
{
    /// <summary>The chosen company type's required identity fields are filled (condition 1).</summary>
    CompanyIdentity,

    /// <summary>At least one payment method is open (condition 2).</summary>
    PaymentMethod,

    /// <summary>The "close sales temporarily" switch is on (condition 3 is the switch being off).</summary>
    TemporaryClosure,

    /// <summary>The privacy notice is complete and published: the identity fields and the hosting location are filled (condition 4; the same definition as the data-collecting entries').</summary>
    PrivacyNotice,

    /// <summary>The cookie policy is published (condition 4).</summary>
    CookiePolicy,

    /// <summary>The return address that the two generated texts carry is entered (condition 4).</summary>
    ReturnAddress,
}

/// <summary>What a surface may do while the gate is closed or open (02 3.1.5, 3.1.6, 6.1.4). The first three are never closed by the gate.</summary>
public enum SalesGateOperation
{
    /// <summary>The storefront and the corporate pages: products stay visible (02 3.1.5).</summary>
    BrowseShowcase,

    /// <summary>The visitor's existing cart: kept and shown as it is (02 3.1.6, 6.1.4).</summary>
    KeepExistingCart,

    /// <summary>An order that is already open: runs on, the bank transfer approval included (02 3.1.6).</summary>
    ContinueOpenOrder,

    /// <summary>Adding a product to the cart.</summary>
    AddToCart,

    /// <summary>Moving from the cart to the payment step.</summary>
    ProceedToCheckout,

    /// <summary>Confirming a new order; a guest order too.</summary>
    ConfirmOrder,
}

/// <summary>An entry that collects personal data (02 3.13.19, 3.32.8, 3.33.9). The sign-in of an existing account collects nothing new and is never closed.</summary>
public enum DataCollectionEntry
{
    /// <summary>The account registration form.</summary>
    AccountRegistration,

    /// <summary>A Google sign-in that would open a new account.</summary>
    NewAccountGoogleSignIn,

    /// <summary>The contact form.</summary>
    ContactForm,

    /// <summary>A sign-in to an existing account, by password or by Google: not affected.</summary>
    ExistingAccountSignIn,
}

/// <summary>
/// The reason a gate answers a refused request with: the code the screen turns into its message and the conditions that are not met, by name, with the identity
/// fields that still wait (02 3.1.5: "the reason and the missing condition are told").
/// </summary>
/// <param name="Code"><see cref="ErrorCode.SalesClosed"/> for the sales operations, <see cref="ErrorCode.DataCollectionClosed"/> for the data-collecting entries.</param>
/// <param name="UnmetConditions">The conditions that decide this refusal, in the enum's order.</param>
/// <param name="MissingIdentityFields">The identity fields the company still waits for; empty when the identity is complete.</param>
public sealed record GateRefusal(ErrorCode Code, IReadOnlyList<SalesGateCondition> UnmetConditions, IReadOnlyList<string> MissingIdentityFields);

/// <summary>
/// The decision of the sales gate and of the gate of the data-collecting entries (02 3.1.5, 3.1.6): one value, computed in one place from the database on every call
/// (<see cref="ISettingsModule.GetSalesGateAsync"/>; 05 4.1.5), never stored. The storefront, the cart, the payment step and the panel's setup checklist all read this value;
/// none of them re-derives the answer from the conditions' sources, so they cannot differ. A condition that no task has bound yet (the payment method before the payment
/// tasks, the privacy notice before the legal text task) is not met. It is a class with an internal constructor and no setter - not a record - so no surface can build a gate
/// of its own or copy one with other conditions: only the gate's rule makes one.
/// </summary>
public sealed class SalesGate
{
    private static readonly SalesGateCondition[] DataCollectionConditions = [SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy];

    internal SalesGate(IReadOnlyList<SalesGateCondition> unmetConditions, IReadOnlyList<string> missingIdentityFields)
    {
        UnmetConditions = unmetConditions;
        MissingIdentityFields = missingIdentityFields;
    }

    /// <summary>Every condition that is not met, in the enum's order; empty when sales are open.</summary>
    public IReadOnlyList<SalesGateCondition> UnmetConditions { get; }

    /// <summary>The identity fields that still wait, by the names of the identity input; empty when the identity is complete.</summary>
    public IReadOnlyList<string> MissingIdentityFields { get; }

    /// <summary>True when all four conditions are met: the products can be added, the payment step opens and an order can be confirmed.</summary>
    public bool IsOpen => UnmetConditions.Count == 0;

    /// <summary>True when the company's "close sales temporarily" switch is on; the panel shows it and the next request carries it as the shown value (07 9.16.3).</summary>
    public bool IsTemporarilyClosed => UnmetConditions.Contains(SalesGateCondition.TemporaryClosure);

    /// <summary>True when the storefront tells the visitor that sales are closed (02 3.1.5); the showcase itself stays published.</summary>
    public bool ShowsClosedNotice => !IsOpen;

    /// <summary>
    /// The panel's setup checklist (02 10.8.2): the unmet conditions that wait for the company to fill something. The temporary closure is the company's own switch
    /// and is not a thing to complete.
    /// </summary>
    public IReadOnlyList<SalesGateCondition> SetupChecklist => [.. UnmetConditions.Where(c => c != SalesGateCondition.TemporaryClosure)];

    /// <summary>True when the data-collecting entries are open: the privacy notice is complete and published and the cookie policy is published (02 3.1.5; 10 ÖK-10).</summary>
    public bool IsDataCollectionOpen => !UnmetConditions.Any(c => DataCollectionConditions.Contains(c));

    /// <summary>Whether <paramref name="operation"/> may be done now. The showcase, the existing cart and an open order are never closed by the gate.</summary>
    public bool Permits(SalesGateOperation operation) => operation switch
    {
        SalesGateOperation.BrowseShowcase or SalesGateOperation.KeepExistingCart or SalesGateOperation.ContinueOpenOrder => true,
        SalesGateOperation.AddToCart or SalesGateOperation.ProceedToCheckout or SalesGateOperation.ConfirmOrder => IsOpen,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Not a sales gate operation."),
    };

    /// <summary>The refusal of <paramref name="operation"/>, or <see langword="null"/> when it may be done.</summary>
    public GateRefusal? RefusalOf(SalesGateOperation operation) =>
        Permits(operation) ? null : new GateRefusal(ErrorCode.SalesClosed, UnmetConditions, MissingIdentityFields);

    /// <summary>Whether <paramref name="entry"/> may be used now. The sign-in of an existing account is never closed.</summary>
    public bool Permits(DataCollectionEntry entry) => entry switch
    {
        DataCollectionEntry.ExistingAccountSignIn => true,
        DataCollectionEntry.AccountRegistration or DataCollectionEntry.NewAccountGoogleSignIn or DataCollectionEntry.ContactForm => IsDataCollectionOpen,
        _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, "Not a data-collecting entry."),
    };

    /// <summary>The refusal of <paramref name="entry"/>, or <see langword="null"/> when it may be used. It names the privacy notice and the cookie policy that wait.</summary>
    public GateRefusal? RefusalOf(DataCollectionEntry entry) =>
        Permits(entry)
            ? null
            : new GateRefusal(ErrorCode.DataCollectionClosed, [.. UnmetConditions.Where(c => DataCollectionConditions.Contains(c))], MissingIdentityFields);
}
