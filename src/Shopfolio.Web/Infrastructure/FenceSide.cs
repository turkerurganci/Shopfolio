namespace Shopfolio.Infrastructure;

/// <summary>Where a firm-set period lies against its fence (02 4.1.5). Only the code names live here; the check lives in <c>PeriodFences</c>.</summary>
public enum FenceSide
{
    Within,
    BelowLower,
    AboveUpper,
}
