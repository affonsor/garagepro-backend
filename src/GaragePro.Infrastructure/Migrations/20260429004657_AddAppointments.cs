using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaragePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expected_end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_rescheduled = table.Column<bool>(type: "boolean", nullable: false),
                    reschedule_count = table.Column<int>(type: "integer", nullable: false),
                    product_value_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    service_value_snapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_appointments", x => x.id);
                    table.ForeignKey(
                        name: "fk_appointments_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_appointments_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_appointments_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appointment_reschedule_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    previous_expected_end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    new_start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    new_expected_end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_appointment_reschedule_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_appointment_reschedule_histories_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_appointment_reschedule_histories_users_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_appointment_reschedule_histories_appointment_id",
                table: "appointment_reschedule_histories",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "ix_appointment_reschedule_histories_changed_by_user_id",
                table: "appointment_reschedule_histories",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_client_id",
                table: "appointments",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_product_id",
                table: "appointments",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_service_id",
                table: "appointments",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_start_at",
                table: "appointments",
                column: "start_at");

            migrationBuilder.CreateIndex(
                name: "ix_appointments_status",
                table: "appointments",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_reschedule_histories");

            migrationBuilder.DropTable(
                name: "appointments");
        }
    }
}
