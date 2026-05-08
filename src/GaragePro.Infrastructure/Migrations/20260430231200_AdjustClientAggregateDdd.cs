using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GaragePro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdjustClientAggregateDdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE addresses SET type = 'Work' WHERE type = 'Billing';");
            migrationBuilder.Sql("UPDATE clients SET document = regexp_replace(document, '[^0-9]', '', 'g') WHERE document IS NOT NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "document",
                table: "clients",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "clients",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "ix_clients_document",
                table: "clients",
                column: "document",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE addresses SET type = 'Billing' WHERE type = 'Work';");

            migrationBuilder.DropIndex(
                name: "ix_clients_document",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "clients");

            migrationBuilder.AlterColumn<string>(
                name: "document",
                table: "clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);
        }
    }
}
