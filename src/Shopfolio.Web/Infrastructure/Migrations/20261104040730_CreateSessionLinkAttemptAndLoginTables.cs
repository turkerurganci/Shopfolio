using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSessionLinkAttemptAndLoginTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attempt_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    attempt_limit = table.Column<string>(type: "text", nullable: false),
                    attempt_axis = table.Column<string>(type: "text", nullable: false),
                    normalized_value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempt_records", x => x.id);
                    table.CheckConstraint("ck_attempt_records_attempt_axis", "\"attempt_axis\" IN ('Email', 'Ip', 'Cart', 'RecognizedBrowser')");
                    table.CheckConstraint("ck_attempt_records_attempt_limit", "\"attempt_limit\" IN ('L1', 'L2', 'L3', 'L4', 'L5', 'L6', 'L9')");
                    table.CheckConstraint("ck_attempt_records_axis_is_the_limits_axis", "(attempt_limit = 'L4' AND attempt_axis = 'Ip') OR (attempt_limit = 'L6' AND attempt_axis = 'Cart') OR (attempt_limit = 'L1' AND attempt_axis = 'RecognizedBrowser') OR (attempt_limit IN ('L1', 'L2', 'L3', 'L5', 'L9') AND attempt_axis IN ('Email', 'Ip'))");
                });

            migrationBuilder.CreateTable(
                name: "link_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    link_type = table.Column<string>(type: "text", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_invitation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    superseded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    previous_email = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link_records", x => x.id);
                    table.CheckConstraint("ck_link_records_at_most_one_of_used_and_superseded", "num_nonnulls(used_at, superseded_at) <= 1");
                    table.CheckConstraint("ck_link_records_customer_only_types_have_customer", "link_type NOT IN ('EmailVerification', 'GoogleLinkConfirmation') OR customer_id IS NOT NULL");
                    table.CheckConstraint("ck_link_records_exactly_one_owner", "num_nonnulls(customer_id, admin_id, admin_invitation_id) = 1");
                    table.CheckConstraint("ck_link_records_invitation_link_only_for_invitation_type", "(link_type = 'AdminInvitation') = (admin_invitation_id IS NOT NULL)");
                    table.CheckConstraint("ck_link_records_link_type", "\"link_type\" IN ('EmailVerification', 'PasswordReset', 'NewEmailVerification', 'EmailChangeRevert', 'AdminInvitation', 'GoogleLinkConfirmation')");
                    table.CheckConstraint("ck_link_records_previous_email_only_for_revert_type", "previous_email IS NULL OR link_type = 'EmailChangeRevert'");
                    table.ForeignKey(
                        name: "fk_link_records_admin_invitations_admin_invitation_id",
                        column: x => x.admin_invitation_id,
                        principalTable: "admin_invitations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_link_records_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_link_records_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "login_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: false),
                    account_type = table.Column<string>(type: "text", nullable: false),
                    normalized_email = table.Column<string>(type: "text", nullable: false),
                    outcome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_login_records", x => x.id);
                    table.CheckConstraint("ck_login_records_account_type", "\"account_type\" IN ('Admin', 'Customer')");
                    table.CheckConstraint("ck_login_records_outcome", "\"outcome\" IN ('Succeeded', 'Failed')");
                });

            migrationBuilder.CreateTable(
                name: "recognized_browser_marks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    mark_hash = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recognized_browser_marks", x => x.id);
                    table.CheckConstraint("ck_recognized_browser_marks_exactly_one_account", "num_nonnulls(customer_id, admin_id) = 1");
                    table.ForeignKey(
                        name: "fk_recognized_browser_marks_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_recognized_browser_marks_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_long_lived = table.Column<bool>(type: "boolean", nullable: false),
                    last_activity_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reauthenticated_operation = table.Column<string>(type: "text", nullable: true),
                    reauthentication_token_hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessions", x => x.id);
                    table.CheckConstraint("ck_sessions_admin_without_account_deletion", "admin_id IS NULL OR reauthenticated_operation IS DISTINCT FROM 'AccountDeletion'");
                    table.CheckConstraint("ck_sessions_exactly_one_account", "num_nonnulls(customer_id, admin_id) = 1");
                    table.CheckConstraint("ck_sessions_reauthenticated_operation", "\"reauthenticated_operation\" IN ('PasswordChange', 'EmailChange', 'AccountDeletion')");
                    table.CheckConstraint("ck_sessions_reauthentication_fields_together", "(reauthenticated_operation IS NULL) = (reauthentication_token_hash IS NULL)");
                    table.ForeignKey(
                        name: "fk_sessions_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_sessions_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attempt_records_attempt_limit_attempt_axis_normalized_value",
                table: "attempt_records",
                columns: new[] { "attempt_limit", "attempt_axis", "normalized_value", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_attempt_records_created_at",
                table: "attempt_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_link_records_admin_id",
                table: "link_records",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_link_records_admin_invitation_id",
                table: "link_records",
                column: "admin_invitation_id");

            migrationBuilder.CreateIndex(
                name: "ix_link_records_customer_id",
                table: "link_records",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_link_records_expires_at",
                table: "link_records",
                column: "expires_at",
                filter: "previous_email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_link_records_link_type_expires_at",
                table: "link_records",
                columns: new[] { "link_type", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_link_records_token_hash",
                table: "link_records",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_login_records_created_at",
                table: "login_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_login_records_ip_address",
                table: "login_records",
                column: "ip_address");

            migrationBuilder.CreateIndex(
                name: "ix_login_records_normalized_email",
                table: "login_records",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_recognized_browser_marks_admin_id",
                table: "recognized_browser_marks",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_recognized_browser_marks_customer_id",
                table: "recognized_browser_marks",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_recognized_browser_marks_expires_at",
                table: "recognized_browser_marks",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_recognized_browser_marks_mark_hash",
                table: "recognized_browser_marks",
                column: "mark_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sessions_admin_id",
                table: "sessions",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_customer_id",
                table: "sessions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_last_activity_at",
                table: "sessions",
                column: "last_activity_at");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_token_hash",
                table: "sessions",
                column: "token_hash",
                unique: true);
        }
    }
}
