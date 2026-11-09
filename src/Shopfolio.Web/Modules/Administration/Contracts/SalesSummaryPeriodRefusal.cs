namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>Why a custom range was refused (04 9.22.9; 07 9.22.1): a date is missing, a date is in the future, or the end is before the start.</summary>
public enum SalesSummaryPeriodRefusal
{
    DateMissing,
    FutureDate,
    EndBeforeStart,
}
