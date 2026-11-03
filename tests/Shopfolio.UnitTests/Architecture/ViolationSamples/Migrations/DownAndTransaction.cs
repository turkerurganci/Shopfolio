// Deliberate violation sample of the migration source scan (09 19.2 item 3): a Down method and a suppressed transaction.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
public partial class DownAndTransactionSample
{
    protected void Up(dynamic migrationBuilder) => migrationBuilder.Sql("SELECT 1", suppressTransaction: true);

    protected void Down(dynamic migrationBuilder) => migrationBuilder.DropTable("orders");
}
