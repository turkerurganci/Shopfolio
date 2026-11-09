namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>The ready periods of the summary's period choice (04 9.22.3; 07 5.3.2 <c>SalesSummaryQuery</c>); the custom range is <see cref="SalesSummaryPeriod.Custom"/>.</summary>
public enum SalesSummaryPreset
{
    Today,
    Yesterday,
    Last7Days,
    Last30Days,
    ThisMonth,
    LastMonth,
    MeasurementWindow,
}
