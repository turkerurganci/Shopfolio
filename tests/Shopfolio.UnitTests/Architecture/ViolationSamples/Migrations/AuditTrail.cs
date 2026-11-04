// Deliberate violation sample of the migration source scan (09 19.2 item 3): a migration that writes data where it must not and touches the audit trail.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
public partial class AuditTrailSample
{
    protected void Up(dynamic migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE audit_logs SET operation = 'x';");
        migrationBuilder.Sql("DELETE FROM public.audit_logs WHERE true;");
        migrationBuilder.Sql("ALTER TABLE public.audit_logs DROP COLUMN details;");
        migrationBuilder.Sql("ALTER TABLE audit_logs RENAME COLUMN operation TO op;");
        migrationBuilder.DropColumn(
            name: "details",
            table: "audit_logs");
        migrationBuilder.Sql("DELETE FROM public.orders WHERE id IS NULL;");
        migrationBuilder.Sql("UPDATE products SET name = 'x';");
        migrationBuilder.Sql("CREATE FUNCTION public.other_function() RETURNS void LANGUAGE sql AS $$ SELECT clock_timestamp() $$;");
        migrationBuilder.Sql("REVOKE ALL ON TABLE t FROM PUBLIC;");
        migrationBuilder.Sql("GRANT EXECUTE ON FUNCTION f(integer) TO PUBLIC;");
    }
}
