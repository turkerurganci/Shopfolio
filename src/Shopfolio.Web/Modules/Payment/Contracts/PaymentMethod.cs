namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>The closed values of PaymentMethod (06 5.3.5). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum PaymentMethod
{
    Card,
    BankTransfer,
}
