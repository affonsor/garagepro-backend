using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaragePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypeAndServiceVehiclePrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "vehicles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Car");

            migrationBuilder.AddColumn<Guid>(
                name: "vehicle_id",
                table: "appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_vehicle_prices",
                columns: table => new
                {
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_vehicle_prices", x => new { x.service_id, x.vehicle_type });
                    table.CheckConstraint("ck_service_vehicle_prices_price", "price >= 0");
                    table.ForeignKey(
                        name: "fk_service_vehicle_prices_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO service_vehicle_prices (service_id, vehicle_type, price)
                SELECT id, 'Car', price
                FROM services
                ON CONFLICT (service_id, vehicle_type) DO NOTHING;
                """);

            migrationBuilder.Sql("""
                UPDATE appointments AS a
                SET vehicle_id = (
                    SELECT v.id
                    FROM vehicles AS v
                    WHERE v.client_id = a.client_id
                    ORDER BY v.created_at, v.id
                    LIMIT 1
                )
                WHERE a.vehicle_id IS NULL;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM appointments WHERE vehicle_id IS NULL) THEN
                        RAISE EXCEPTION 'Cannot backfill appointments.vehicle_id because some appointment clients have no vehicles.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "vehicle_id",
                table: "appointments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_appointments_vehicle_id",
                table: "appointments",
                column: "vehicle_id");

            migrationBuilder.AddForeignKey(
                name: "fk_appointments_vehicles_vehicle_id",
                table: "appointments",
                column: "vehicle_id",
                principalTable: "vehicles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_appointments_vehicles_vehicle_id",
                table: "appointments");

            migrationBuilder.DropTable(
                name: "service_vehicle_prices");

            migrationBuilder.DropIndex(
                name: "ix_appointments_vehicle_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "type",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "vehicle_id",
                table: "appointments");
        }
    }
}
