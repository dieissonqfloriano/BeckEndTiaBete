using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsentimentoLgpd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentimentoResponsavelEm",
                table: "Usuarios",
                type: "datetime(6)",
                precision: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentimentoSaudeEm",
                table: "Usuarios",
                type: "datetime(6)",
                precision: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsavelNome",
                table: "Usuarios",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "TermosAceitosEm",
                table: "Usuarios",
                type: "datetime(6)",
                precision: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersaoTermos",
                table: "Usuarios",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentimentoResponsavelEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ConsentimentoSaudeEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ResponsavelNome",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TermosAceitosEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "VersaoTermos",
                table: "Usuarios");
        }
    }
}
