// Deliberate violation sample of the enum file scan (09 19.2 item 3): a rule written next to the enum.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
namespace Shopfolio.Samples;

public enum EnumWithMethod
{
    First,
}

// The violation is on line 11.
public static class EnumWithMethodRules
{
    public static bool IsAllowed(EnumWithMethod value) => value == EnumWithMethod.First;
}
