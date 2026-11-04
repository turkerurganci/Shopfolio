// Deliberate violation sample of the data model source scan (09 19.2 item 3): a module type name written as a plain string inside a HasOne call.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 6.
public static class ModuleTypeNameInHasOneSample
{
    public static void Configure(dynamic builder) => builder.HasOne("Shopfolio.Modules.Administration.Admin", navigationName: null);
}
