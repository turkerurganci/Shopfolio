using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAdminInvitationAndAuditLogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    normalized_email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admins", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_name = table.Column<string>(type: "text", nullable: false),
                    admin_email = table.Column<string>(type: "text", nullable: false),
                    audit_area = table.Column<string>(type: "text", nullable: false),
                    operation = table.Column<string>(type: "text", nullable: false),
                    target_record_type = table.Column<string>(type: "text", nullable: true),
                    target_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                    table.CheckConstraint("ck_audit_logs_audit_area", "\"audit_area\" IN ('CompanyAndBranding', 'Publication', 'Order', 'AdminAccounts', 'LegalTextVersion', 'Settings', 'Export', 'CustomerDeletion')");
                });

            migrationBuilder.CreateTable(
                name: "admin_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    normalized_email = table.Column<string>(type: "text", nullable: false),
                    sender_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sender_admin_name = table.Column<string>(type: "text", nullable: false),
                    withdrawn_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    invalidated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_invitations", x => x.id);
                    table.CheckConstraint("ck_admin_invitations_at_most_one_outcome", "num_nonnulls(withdrawn_at, invalidated_at, accepted_at) <= 1");
                    table.ForeignKey(
                        name: "fk_admin_invitations_admins_sender_admin_id",
                        column: x => x.sender_admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_invitations_normalized_email",
                table: "admin_invitations",
                column: "normalized_email",
                unique: true,
                filter: "withdrawn_at IS NULL AND invalidated_at IS NULL AND accepted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_admin_invitations_sender_admin_id",
                table: "admin_invitations",
                column: "sender_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_admins_normalized_email",
                table: "admins",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_admin_id_created_at",
                table: "audit_logs",
                columns: new[] { "admin_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");

            // The audit trail is insert and read only for the application user (06 4.63; 09 10.8 item 3). The default privileges of the
            // schema gave the user all four; the two are taken back here. The user name is read from the schema owner's role setting
            // (09 10.8 item 5), never written here.
            migrationBuilder.Sql(@"DO $$ BEGIN
  EXECUTE format('REVOKE UPDATE, DELETE ON public.audit_logs FROM %I', current_setting('shopfolio.application_user'));
END $$;");

            // The disposal function (09 9.2 items 6, 7): the one way an expired audit row leaves the table. It runs with its owner's
            // privileges (the schema owner), its search path is fixed, and its clock read is the one upper bound on the given moment.
            migrationBuilder.Sql(@"CREATE FUNCTION public.dispose_expired_audit_logs(max_rows integer, as_of timestamptz)
RETURNS integer LANGUAGE plpgsql SECURITY DEFINER
SET search_path = pg_catalog, public
AS $$
DECLARE
  cutoff timestamptz;
  removed integer;
BEGIN
  IF max_rows < 1 THEN RAISE EXCEPTION 'max_rows must be positive'; END IF;
  as_of := least(as_of, clock_timestamp());
  cutoff := make_timestamp(
      extract(year FROM ((as_of AT TIME ZONE 'UTC') + interval '3 hours'))::integer - 10,
      1, 1, 0, 0, 0) AT TIME ZONE 'UTC' - interval '3 hours';
  WITH expired AS (
    SELECT id FROM public.audit_logs WHERE created_at < cutoff ORDER BY created_at LIMIT max_rows)
  DELETE FROM public.audit_logs AS log USING expired WHERE log.id = expired.id;
  GET DIAGNOSTICS removed = ROW_COUNT;
  RETURN removed;
END $$;");
            migrationBuilder.Sql("REVOKE ALL ON FUNCTION public.dispose_expired_audit_logs(integer, timestamptz) FROM PUBLIC;");
            migrationBuilder.Sql(@"DO $$ BEGIN
  EXECUTE format('GRANT EXECUTE ON FUNCTION public.dispose_expired_audit_logs(integer, timestamptz) TO %I', current_setting('shopfolio.application_user'));
END $$;");
        }
    }
}
