using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateDispatchAndChannelStateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channel_state_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_succeeded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    consecutive_channel_error_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    first_channel_error_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_state_records", x => x.id);
                    table.CheckConstraint("ck_channel_state_records_error_count_not_negative", "consecutive_channel_error_count >= 0");
                    table.CheckConstraint("ck_channel_state_records_first_error_at_with_count", "(consecutive_channel_error_count = 0) = (first_channel_error_at IS NULL)");
                });

            migrationBuilder.CreateTable(
                name: "dispatch_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    dispatch_type = table.Column<string>(type: "text", nullable: false),
                    recipient_email = table.Column<string>(type: "text", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_record_type = table.Column<string>(type: "text", nullable: true),
                    related_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    original_dispatch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_values = table.Column<string>(type: "jsonb", nullable: true),
                    dispatch_state = table.Column<string>(type: "text", nullable: false, defaultValue: "Queued"),
                    last_outcome = table.Column<string>(type: "text", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dispatch_records", x => x.id);
                    table.CheckConstraint("ck_dispatch_records_attempt_count_not_negative", "attempt_count >= 0");
                    table.CheckConstraint("ck_dispatch_records_dispatch_state", "\"dispatch_state\" IN ('Queued', 'Sent', 'Undelivered')");
                    table.CheckConstraint("ck_dispatch_records_dispatch_type", "\"dispatch_type\" IN ('B1', 'B2', 'B3', 'B4', 'B5', 'B6', 'B7', 'B8', 'B9', 'B10', 'B11', 'B12', 'B13', 'B14', 'B15', 'B16', 'F1', 'F2', 'F3', 'F4', 'F5', 'F6', 'EmailVerificationLink', 'PasswordResetLink', 'NewEmailVerificationLink', 'EmailChangeNotice', 'AdminInvitation', 'AccountDeletedNotice', 'GoogleLinkConfirmationLink')");
                    table.CheckConstraint("ck_dispatch_records_last_outcome", "\"last_outcome\" IN ('Sent', 'RecipientError', 'ChannelError')");
                    table.CheckConstraint("ck_dispatch_records_next_attempt_only_when_queued", "dispatch_state = 'Queued' OR next_attempt_at IS NULL");
                    table.CheckConstraint("ck_dispatch_records_order_and_related_record_by_type", "CASE dispatch_type WHEN 'B1' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B2' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B3' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B4' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B5' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B6' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B7' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B8' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B10' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B11' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B12' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B13' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B14' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B15' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B16' THEN order_id IS NOT NULL AND related_record_type IS NULL WHEN 'B9' THEN order_id IS NOT NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Withdrawal', 'LateDeliveryTermination', 'DefectClaim') WHEN 'F1' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Order') WHEN 'F2' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('ContactRequest') WHEN 'F3' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Withdrawal', 'LateDeliveryTermination', 'DefectClaim') WHEN 'F4' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Refund') WHEN 'F5' THEN order_id IS NULL AND related_record_type IS NULL WHEN 'F6' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('AdminInvitation', 'Admin') WHEN 'EmailVerificationLink' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Customer') WHEN 'AccountDeletedNotice' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Customer') WHEN 'GoogleLinkConfirmationLink' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Customer') WHEN 'PasswordResetLink' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Customer', 'Admin') WHEN 'NewEmailVerificationLink' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Customer', 'Admin') WHEN 'EmailChangeNotice' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('Customer', 'Admin') WHEN 'AdminInvitation' THEN order_id IS NULL AND related_record_type IS NOT NULL AND related_record_type IN ('AdminInvitation') ELSE false END");
                    table.CheckConstraint("ck_dispatch_records_related_record_type", "\"related_record_type\" IN ('Order', 'Withdrawal', 'LateDeliveryTermination', 'DefectClaim', 'Refund', 'ContactRequest', 'Customer', 'Admin', 'AdminInvitation')");
                    table.CheckConstraint("ck_dispatch_records_related_record_type_with_id", "(related_record_type IS NULL) = (related_record_id IS NULL)");
                    table.CheckConstraint("ck_dispatch_records_sent_at_with_state", "(dispatch_state <> 'Sent' OR sent_at IS NOT NULL) AND (dispatch_state <> 'Queued' OR sent_at IS NULL)");
                    table.ForeignKey(
                        name: "fk_dispatch_records_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_records_created_at",
                table: "dispatch_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_records_dispatch_state_next_attempt_at",
                table: "dispatch_records",
                columns: new[] { "dispatch_state", "next_attempt_at" },
                filter: "dispatch_state = 'Queued'");

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_records_dispatch_type_created_at",
                table: "dispatch_records",
                columns: new[] { "dispatch_type", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_records_order_id_created_at",
                table: "dispatch_records",
                columns: new[] { "order_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_records_related_record_type_related_record_id_crea",
                table: "dispatch_records",
                columns: new[] { "related_record_type", "related_record_id", "created_at" });

            // The one row of ChannelStateRecord (06 2.3.2): a unique index on a constant expression allows a second row never. EF cannot model
            // an expression index, so it is written here by hand (09 10.6 item 1).
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_channel_state_records_single_row ON public.channel_state_records ((true));");
        }
    }
}
