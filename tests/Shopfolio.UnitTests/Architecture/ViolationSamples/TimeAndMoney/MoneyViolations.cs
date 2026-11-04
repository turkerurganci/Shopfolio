// Deliberate violations of the money rules (09 14.1 items 1, 5; 05 7.2.5). Compiled into TimeAndMoneyViolationSample.dll.
using System;
using System.Numerics;

namespace Shopfolio.Modules.Sample
{
    public static class RoundingOutsideMoney
    {
        public static decimal MathRound(decimal amount) => Math.Round(amount, 2);

        public static decimal MathRoundAwayFromZero(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        public static decimal DecimalRound(decimal amount) => decimal.Round(amount, 2);

        public static decimal MathFloor(decimal amount) => Math.Floor(amount);

        public static decimal MathCeiling(decimal amount) => Math.Ceiling(amount);

        public static decimal MathTruncate(decimal amount) => Math.Truncate(amount);

        public static decimal DecimalFloor(decimal amount) => decimal.Floor(amount);

        public static double DoubleRound(double amount) => double.Round(amount, 2, MidpointRounding.AwayFromZero);

        // A rounding call through generic math names no Math or decimal member; its MidpointRounding argument is what gives it away.
        public static T GenericRound<T>(T amount)
            where T : IFloatingPoint<T> => T.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    // A floating point amount in a module: a field, a property, a parameter and a return.
    public sealed class FloatingPrice
    {
        public double Price;

        public float Discount { get; set; }

        public static decimal Total(double amount) => (decimal)amount;

        public static float Share(decimal amount) => (float)amount;
    }
}

// The exemption is the one class by its name: a class beside it that rounds is caught, the class itself is not.
namespace Shopfolio.Infrastructure
{
    public static class Money
    {
        public static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public static class MoneyHelper
    {
        public static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    // The time source exemption likewise: TimeSource may name TimeProvider.System, a look-alike may not.
    public static class TimeSource
    {
        public static TimeProvider Clock() => TimeProvider.System;
    }

    public static class TimeSourceCopy
    {
        public static TimeProvider Clock() => TimeProvider.System;
    }
}
