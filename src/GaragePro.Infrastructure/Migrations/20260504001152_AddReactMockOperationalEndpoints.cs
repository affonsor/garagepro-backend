using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaragePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReactMockOperationalEndpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "services",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "services",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cost",
                table: "services",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "duration",
                table: "services",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tier",
                table: "services",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "standard");

            migrationBuilder.AddColumn<string>(
                name: "barcode",
                table: "products",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "brand",
                table: "products",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "products",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cost",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "min_stock",
                table: "products",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "size",
                table: "products",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sku",
                table: "products",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "stock",
                table: "products",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "supplier",
                table: "products",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unit",
                table: "products",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "un");

            migrationBuilder.AddColumn<string>(
                name: "address_text",
                table: "clients",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "birthday",
                table: "clients",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "clients",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tier",
                table: "clients",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "standard");

            migrationBuilder.CreateTable(
                name: "service_materials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_materials", x => x.id);
                    table.CheckConstraint("ck_service_materials_quantity", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_service_materials_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_materials_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    box_number = table.Column<int>(type: "integer", nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_orders", x => x.id);
                    table.CheckConstraint("ck_service_orders_box_number", "box_number between 1 and 6");
                    table.CheckConstraint("ck_service_orders_total_price", "total_price >= 0");
                    table.ForeignKey(
                        name: "fk_service_orders_clients_client_id",
                        column: x => x.client_id,
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_orders_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "service_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_steps_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_product_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_order_product_lines", x => x.id);
                    table.CheckConstraint("ck_service_order_product_lines_line_total", "line_total >= 0");
                    table.CheckConstraint("ck_service_order_product_lines_quantity", "quantity > 0");
                    table.CheckConstraint("ck_service_order_product_lines_unit_price", "unit_price >= 0");
                    table.ForeignKey(
                        name: "fk_service_order_product_lines_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_order_product_lines_service_orders_service_order_id",
                        column: x => x.service_order_id,
                        principalTable: "service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_service_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_order_service_lines", x => x.id);
                    table.CheckConstraint("ck_service_order_service_lines_line_total", "line_total >= 0");
                    table.CheckConstraint("ck_service_order_service_lines_quantity", "quantity > 0");
                    table.CheckConstraint("ck_service_order_service_lines_unit_price", "unit_price >= 0");
                    table.ForeignKey(
                        name: "fk_service_order_service_lines_service_orders_service_order_id",
                        column: x => x.service_order_id,
                        principalTable: "service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_order_service_lines_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_services_code",
                table: "services",
                column: "code",
                unique: true,
                filter: "code IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_services_cost",
                table: "services",
                sql: "cost >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_products_sku",
                table: "products",
                column: "sku",
                unique: true,
                filter: "sku IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_products_cost",
                table: "products",
                sql: "cost >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_products_min_stock",
                table: "products",
                sql: "min_stock >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_products_stock",
                table: "products",
                sql: "stock >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_service_materials_product_id",
                table: "service_materials",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_materials_service_id_product_id",
                table: "service_materials",
                columns: new[] { "service_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_order_product_lines_product_id",
                table: "service_order_product_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_order_product_lines_service_order_id",
                table: "service_order_product_lines",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_order_service_lines_service_id",
                table: "service_order_service_lines",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_order_service_lines_service_order_id",
                table: "service_order_service_lines",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_client_id",
                table: "service_orders",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_order_number",
                table: "service_orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_scheduled_at",
                table: "service_orders",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_status",
                table: "service_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_vehicle_id",
                table: "service_orders",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_steps_service_id_position",
                table: "service_steps",
                columns: new[] { "service_id", "position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_materials");

            migrationBuilder.DropTable(
                name: "service_order_product_lines");

            migrationBuilder.DropTable(
                name: "service_order_service_lines");

            migrationBuilder.DropTable(
                name: "service_steps");

            migrationBuilder.DropTable(
                name: "service_orders");

            migrationBuilder.DropIndex(
                name: "ix_services_code",
                table: "services");

            migrationBuilder.DropCheckConstraint(
                name: "ck_services_cost",
                table: "services");

            migrationBuilder.DropIndex(
                name: "ix_products_sku",
                table: "products");

            migrationBuilder.DropCheckConstraint(
                name: "ck_products_cost",
                table: "products");

            migrationBuilder.DropCheckConstraint(
                name: "ck_products_min_stock",
                table: "products");

            migrationBuilder.DropCheckConstraint(
                name: "ck_products_stock",
                table: "products");

            migrationBuilder.DropColumn(
                name: "category",
                table: "services");

            migrationBuilder.DropColumn(
                name: "code",
                table: "services");

            migrationBuilder.DropColumn(
                name: "cost",
                table: "services");

            migrationBuilder.DropColumn(
                name: "duration",
                table: "services");

            migrationBuilder.DropColumn(
                name: "tier",
                table: "services");

            migrationBuilder.DropColumn(
                name: "barcode",
                table: "products");

            migrationBuilder.DropColumn(
                name: "brand",
                table: "products");

            migrationBuilder.DropColumn(
                name: "category",
                table: "products");

            migrationBuilder.DropColumn(
                name: "cost",
                table: "products");

            migrationBuilder.DropColumn(
                name: "min_stock",
                table: "products");

            migrationBuilder.DropColumn(
                name: "size",
                table: "products");

            migrationBuilder.DropColumn(
                name: "sku",
                table: "products");

            migrationBuilder.DropColumn(
                name: "stock",
                table: "products");

            migrationBuilder.DropColumn(
                name: "supplier",
                table: "products");

            migrationBuilder.DropColumn(
                name: "unit",
                table: "products");

            migrationBuilder.DropColumn(
                name: "address_text",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "birthday",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "tier",
                table: "clients");
        }
    }
}
