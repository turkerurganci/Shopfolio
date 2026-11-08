namespace Shopfolio.Modules.Catalog.Contracts.Pricing;

/// <summary>The state of a product's discount, read from its dates (04 9.5.8; 03 1.10.1; 06 4.12): not started, running or ended. Not stored; only the code names live here.</summary>
public enum DiscountState
{
    NotStarted,
    Running,
    Ended,
}
