// Deliberate violation sample of the start-up data source scan (09 19.2 item 3): rows a migration never writes.
// This file is excluded from compilation and from the real scan; the scan test reads it as text. The violations are on lines 7, 8 and 9.
public partial class SampleMigration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("INSERT INTO public.data_protection_keys (friendly_name, xml) VALUES ('k', '<key/>');");
        migrationBuilder.InsertData(table: "data_protection_keys", columns: ["xml"], values: ["<key/>"]);
        migrationBuilder.Sql("INSERT INTO public.legal_text_versions (id, created_at, legal_text_type, text)\n"
            + "VALUES ('x', 'y', 'PreInformationForm', '<p>x</p>');");
    }
}
