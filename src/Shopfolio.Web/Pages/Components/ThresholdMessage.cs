using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Pages.Components;

/// <summary>Which threshold message (04 2.6.5): the free shipping line, or the minimum order's missing amount.</summary>
public enum ThresholdKind
{
    FreeShipping,
    MinimumOrder,
}

/// <summary>One threshold message: a lasting message in its place - it closes by nothing and is no timed announcement (04 2.6.5; 02 3.19.5).</summary>
public sealed record ThresholdMessage(ThresholdKind Kind, string Text);

/// <summary>
/// The threshold messages of the cart and of the payment step's summary (T542; 04 2.6.5; 02 3.19.5, 3.18.1): the text is the shipping assessment's own (<see cref="ShippingAssessment"/>), so the
/// cart and the summary say the same sentence and the rule stays in the Cart and order module. A basket with no condition gets no message at all - null, and the screen draws nothing. The free
/// shipping line sits inside the amount's breakdown under the shipping fee; the minimum order line sits under the breakdown, in the place of the step's button.
/// </summary>
public static class ThresholdMessages
{
    /// <summary>The free shipping line: "Ücretsiz kargoya 80 TL kaldı" or "Kargo ücretsiz"; null without a threshold or without a physical item.</summary>
    public static ThresholdMessage? FreeShipping(ShippingAssessment assessment)
    {
        ArgumentNullException.ThrowIfNull(assessment);
        return assessment.FreeShippingMessage is string text ? new ThresholdMessage(ThresholdKind.FreeShipping, text) : null;
    }

    /// <summary>The missing amount under the minimum order; null when the order may be placed.</summary>
    public static ThresholdMessage? MinimumOrder(ShippingAssessment assessment)
    {
        ArgumentNullException.ThrowIfNull(assessment);
        return assessment.MinimumOrderMessage is string text ? new ThresholdMessage(ThresholdKind.MinimumOrder, text) : null;
    }
}
