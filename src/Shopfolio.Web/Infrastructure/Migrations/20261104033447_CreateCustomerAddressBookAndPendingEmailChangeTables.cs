using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCustomerAddressBookAndPendingEmailChangeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    normalized_email = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    google_linked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                    table.CheckConstraint("ck_customers_unverified_has_password_and_no_google", "email_verified_at IS NOT NULL OR (password_hash IS NOT NULL AND google_linked_at IS NULL)");
                });

            migrationBuilder.CreateTable(
                name: "address_books",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    recipient_name = table.Column<string>(type: "text", nullable: false),
                    province_id = table.Column<Guid>(type: "uuid", nullable: false),
                    district = table.Column<string>(type: "text", nullable: false),
                    address_line = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address_books", x => x.id);
                    table.CheckConstraint("ck_address_books_phone_ten_digits", "phone ~ '^[0-9]{10}$'");
                    table.ForeignKey(
                        name: "fk_address_books_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_address_books_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pending_email_changes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    new_email = table.Column<string>(type: "text", nullable: false),
                    normalized_new_email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pending_email_changes", x => x.id);
                    table.CheckConstraint("ck_pending_email_changes_exactly_one_account", "num_nonnulls(customer_id, admin_id) = 1");
                    table.ForeignKey(
                        name: "fk_pending_email_changes_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pending_email_changes_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_address_books_customer_id",
                table: "address_books",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_address_books_province_id",
                table: "address_books",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_normalized_email",
                table: "customers",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pending_email_changes_admin_id",
                table: "pending_email_changes",
                column: "admin_id",
                unique: true,
                filter: "admin_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_pending_email_changes_customer_id",
                table: "pending_email_changes",
                column: "customer_id",
                unique: true,
                filter: "customer_id IS NOT NULL");
        }
    }
}
