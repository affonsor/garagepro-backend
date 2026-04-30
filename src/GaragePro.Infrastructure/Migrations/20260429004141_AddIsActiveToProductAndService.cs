using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaragePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToProductAndService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "services",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "services");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "products");
        }
    }
}
