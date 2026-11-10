// Deliberate violations of the provider amount rule (09 14.2 item 4; T403). Compiled into TimeAndMoneyViolationSample.dll.
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Shopfolio.Infrastructure.Integrations
{
    // A layer class beside the conversion class that reads a provider amount itself: each form of the call is caught.
    public static class ProviderAmountReader
    {
        public static decimal Parse(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);

        public static decimal ParseSpan(string text) => decimal.Parse(text.AsSpan(), NumberStyles.Number, CultureInfo.InvariantCulture);

        public static bool TryParse(string text, out decimal value) => decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

        public static decimal Convert(string text) => System.Convert.ToDecimal(text, CultureInfo.InvariantCulture);

        public static decimal FromJson(JsonElement element) => element.GetDecimal();

        public static bool TryFromJson(JsonElement element, out decimal value) => element.TryGetDecimal(out value);

        // A method group and a lambda name the same member.
        public static decimal[] ParseAll(string[] texts) => texts.Select(decimal.Parse).ToArray();

        public static decimal[] ParseAllInLambda(string[] texts) => texts.Select(t => decimal.Parse(t, CultureInfo.InvariantCulture)).ToArray();
    }

    // The exemption is the one class by its name.
    public static class PaymentAmounts
    {
        public static decimal? TryParseProviderText(string text) =>
            decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal value) ? value : null;
    }
}

// Outside the layer the rule does not apply (09 14.2 item 4 names the layer's files).
namespace Shopfolio.Infrastructure.Validation
{
    public static class FormAmount
    {
        public static decimal Parse(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);
    }
}
