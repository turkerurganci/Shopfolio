// Deliberate violation sample of the enum file scan (09 19.2 item 3): a number given to a code name.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 8.
namespace Shopfolio.Samples;

public enum EnumWithValues
{
    First = 1,
}
