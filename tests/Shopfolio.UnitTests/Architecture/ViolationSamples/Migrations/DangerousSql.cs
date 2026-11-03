// Deliberate violation sample of the migration source scan (09 19.2 item 3): statements a migration must not hold.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
public partial class DangerousSqlSample
{
    protected void Up(dynamic migrationBuilder)
    {
        migrationBuilder.Sql("CREATE INDEX CONCURRENTLY ix_a ON t (a);");
        migrationBuilder.Sql("VACUUM t;");
        migrationBuilder.Sql("ALTER SYSTEM SET work_mem = '64MB';");
        migrationBuilder.Sql("CREATE DATABASE other;");
        migrationBuilder.Sql("CREATE EXTENSION pgcrypto;");
        migrationBuilder.Sql("UPDATE t SET changed_at = now();");
    }
}
