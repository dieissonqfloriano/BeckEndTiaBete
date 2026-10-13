using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmacaoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CodigoConfirmacaoExpiraEm",
                table: "Usuarios",
                type: "datetime(6)",
                precision: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoConfirmacaoHash",
                table: "Usuarios",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmado",
                table: "Usuarios",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TentativasCodigo",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoConfirmacaoExpiraEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CodigoConfirmacaoHash",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailConfirmado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TentativasCodigo",
                table: "Usuarios");
        }
    }
}
