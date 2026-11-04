// Deliberate violation sample of the data model source scan (09 19.2 item 3): a constant that holds a module type name is used outside HasOne.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 6.
public static class ModuleTypeNameConstantSample
{
    public static readonly string Used = AdminType;
    private const string AdminType = "Shopfolio.Modules.Administration.Admin";
}
