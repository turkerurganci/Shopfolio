// Clean sample of the migration source scan (09 19.2 item 3): forward only, privileges read from the role settings.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
public partial class CleanSample
{
    protected void Up(dynamic migrationBuilder)
    {
        migrationBuilder.Sql("CREATE UNIQUE INDEX ix_t_single_row ON public.t ((true));");
        migrationBuilder.Sql("EXECUTE format('REVOKE ALL ON TABLE t FROM %I', current_setting('shopfolio.application_user'));");
    }
}
