using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSchedulerBackupDisposalAndKeyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "backup_status_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_succeeded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_backup_status_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "disposal_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    disposal_record_type = table.Column<string>(type: "text", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: true),
                    range_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    range_end_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_disposal_records", x => x.id);
                    table.CheckConstraint("ck_disposal_records_count_and_range_by_type", "CASE WHEN disposal_record_type = 'ServerLogRotation' THEN count IS NULL AND range_start_date IS NOT NULL AND range_end_date IS NOT NULL ELSE count IS NOT NULL AND count > 0 AND range_start_date IS NULL AND range_end_date IS NULL END");
                    table.CheckConstraint("ck_disposal_records_disposal_record_type", "\"disposal_record_type\" IN ('Order', 'DispatchRecord', 'ContactRequest', 'LoginRecord', 'AuditLog', 'DisposalRecord', 'InboundNotificationRecord', 'DefectClaim', 'Image', 'DigitalFile', 'LogFile', 'ServerLogRotation', 'OrphanFile', 'Customer', 'Admin', 'PendingRegistration', 'RefundIban', 'PendingEmailChange', 'AttemptRecord', 'Session', 'RecognizedBrowserMark', 'Cart', 'AdminInvitation', 'LinkRecord')");
                    table.CheckConstraint("ck_disposal_records_range_end_not_before_start", "range_end_date >= range_start_date");
                });

            migrationBuilder.CreateTable(
                name: "heartbeat_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_heartbeat_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nightly_job_last_succeeded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    nightly_job_last_attempted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_heartbeat_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    job_type = table.Column<string>(type: "text", nullable: false),
                    related_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    job_state = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error_summary = table.Column<string>(type: "text", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jobs", x => x.id);
                    table.CheckConstraint("ck_jobs_attempt_count_not_negative", "attempt_count >= 0");
                    table.CheckConstraint("ck_jobs_completed_at_with_state", "(job_state = 'Completed') = (completed_at IS NOT NULL)");
                    table.CheckConstraint("ck_jobs_job_state", "\"job_state\" IN ('Pending', 'Completed', 'Failed')");
                    table.CheckConstraint("ck_jobs_job_type", "\"job_type\" IN ('CardRefund', 'FinalPaymentQuery', 'ImageDerivative', 'InboundNotificationProcessing', 'RefundOutcomeQuery', 'BounceQuery')");
                    table.CheckConstraint("ck_jobs_related_record_by_type", "(job_type = 'BounceQuery') = (related_record_id IS NULL)");
                });

            migrationBuilder.CreateTable(
                name: "outage_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outage_records", x => x.id);
                    table.CheckConstraint("ck_outage_records_longer_than_two_minutes", "ended_at - started_at > interval '2 minutes'");
                });

            migrationBuilder.CreateIndex(
                name: "ix_disposal_records_created_at",
                table: "disposal_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_completed_at",
                table: "jobs",
                column: "completed_at",
                filter: "job_state = 'Completed'");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_job_state",
                table: "jobs",
                column: "job_state",
                filter: "job_state = 'Failed'");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_job_state_next_attempt_at",
                table: "jobs",
                columns: new[] { "job_state", "next_attempt_at" },
                filter: "job_state = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_jobs_job_type_related_record_id",
                table: "jobs",
                columns: new[] { "job_type", "related_record_id" },
                unique: true,
                filter: "job_state = 'Pending'")
                .Annotation("Npgsql:NullsDistinct", false);

            // The one row of HeartbeatRecord and BackupStatusRecord (06 2.3.2): a unique index on a constant expression allows a second row
            // never. EF cannot model an expression index, so it is written here by hand (09 10.6 item 1).
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_heartbeat_records_single_row ON public.heartbeat_records ((true));");
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_backup_status_records_single_row ON public.backup_status_records ((true));");

            // Two of the three tables that differ from the default privileges (09 10.8 item 3): the key ring is closed to the read-only user
            // (05 7.1.3; K-1082), and the backup status user may update only the two moments of the status row (06 4.67) — no SELECT, so the
            // scheduler's update has no WHERE. The user names are read from the schema owner's role settings (09 10.8 item 5), never written here.
            migrationBuilder.Sql(@"DO $$ BEGIN
  EXECUTE format('REVOKE SELECT ON public.data_protection_keys FROM %I', current_setting('shopfolio.reader_user'));
END $$;");
            migrationBuilder.Sql(@"DO $$ BEGIN
  EXECUTE format('GRANT UPDATE (last_succeeded_at, last_failed_at) ON public.backup_status_records TO %I', current_setting('shopfolio.backup_status_user'));
END $$;");
        }
    }
}
