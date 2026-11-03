// Deliberate violation sample of the migration source scan (09 19.2 item 3): a fixed user name and privilege statements outside the role settings.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
public partial class PrivilegesSample
{
    protected void Up(dynamic migrationBuilder)
    {
        migrationBuilder.Sql("REVOKE ALL ON t FROM shopfolio_app;");
        migrationBuilder.Sql("GRANT SELECT ON t TO PUBLIC;");
    }
}
