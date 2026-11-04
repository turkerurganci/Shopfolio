// Clean sample of the migration source scan (09 19.2 item 3): forward only, privileges read from the role settings, the disposal function as the one clock read, the one delete from the audit trail and the one revoke from PUBLIC.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
public partial class CleanSample
{
    protected void Up(dynamic migrationBuilder)
    {
        migrationBuilder.Sql("CREATE UNIQUE INDEX ix_t_single_row ON public.t ((true));");
        migrationBuilder.Sql("EXECUTE format('REVOKE ALL ON TABLE t FROM %I', current_setting('shopfolio.application_user'));");
        migrationBuilder.Sql("UPDATE public.company_settings SET version = version + 1;");
        migrationBuilder.Sql(@"DO $$ BEGIN
  EXECUTE format('REVOKE UPDATE, DELETE ON public.audit_logs FROM %I', current_setting('shopfolio.application_user'));
END $$;");
        migrationBuilder.Sql(@"CREATE FUNCTION public.dispose_expired_audit_logs(max_rows integer, as_of timestamptz)
RETURNS integer LANGUAGE plpgsql SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
BEGIN
  as_of := least(as_of, clock_timestamp());
  WITH expired AS (
    SELECT id FROM public.audit_logs WHERE created_at < as_of ORDER BY created_at LIMIT max_rows)
  DELETE FROM public.audit_logs AS log USING expired WHERE log.id = expired.id;
  RETURN 0;
END $$;");
        migrationBuilder.Sql("REVOKE ALL ON FUNCTION public.dispose_expired_audit_logs(integer, timestamptz) FROM PUBLIC;");
        migrationBuilder.Sql(@"DO $$ BEGIN
  EXECUTE format('GRANT EXECUTE ON FUNCTION public.dispose_expired_audit_logs(integer, timestamptz) TO %I', current_setting('shopfolio.application_user'));
END $$;");
        migrationBuilder.Sql("ALTER TABLE public.audit_logs ADD COLUMN extra text;");
        migrationBuilder.DropColumn(
            name: "old",
            table: "other_table");
    }
}
