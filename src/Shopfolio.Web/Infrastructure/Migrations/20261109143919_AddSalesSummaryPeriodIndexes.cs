using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <summary>
    /// Expansion (09 10.6 item 3; 09 20 item 5; T383): the sales summary reads its period on axes no earlier query filtered on - the orders born in the period, the payment
    /// approvals (O1) of the period, the item records of a loss by their day and the contact requests opened in the period (02 10.6.2, 10.6.3) - so each axis brings its index
    /// (06 7.5.1). Indexes only; no row changes and the previous version's code runs on it. There is no <c>Down</c> (09 10.6 item 1).
    /// </summary>
    public partial class AddSalesSummaryPeriodIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_withdrawals_declaration_date",
                table: "withdrawals",
                column: "declaration_date");

            migrationBuilder.CreateIndex(
                name: "ix_transition_records_transition_code_created_at",
                table: "transition_records",
                columns: new[] { "transition_code", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_late_delivery_terminations_declaration_date",
                table: "late_delivery_terminations",
                column: "declaration_date");

            migrationBuilder.CreateIndex(
                name: "ix_item_removals_created_at",
                table: "item_removals",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_contact_requests_created_at",
                table: "contact_requests",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_cancellations_created_at",
                table: "cancellations",
                column: "created_at");
        }
    }
}
