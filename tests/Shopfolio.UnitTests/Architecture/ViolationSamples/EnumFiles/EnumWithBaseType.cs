// Deliberate violation sample of the enum file scan (09 19.2 item 3): an enum declared with a base type (D-48 b) must still be scanned.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
namespace Shopfolio.Samples;

public enum EnumWithBaseType : int
{
    First,
    // The violation is on line 9.
    Second = 2,
}
