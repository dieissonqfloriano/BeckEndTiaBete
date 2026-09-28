using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SenhaHash = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoDiabetes = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FatorSensibilidade = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: false),
                    HgtAlvo = table.Column<int>(type: "int", nullable: false),
                    Idade = table.Column<int>(type: "int", nullable: true),
                    Celular = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DesativadoEm = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.CheckConstraint("CK_Usuarios_FatorSensibilidade", "`FatorSensibilidade` BETWEEN 1 AND 600");
                    table.CheckConstraint("CK_Usuarios_HgtAlvo", "`HgtAlvo` BETWEEN 1 AND 600");
                    table.CheckConstraint("CK_Usuarios_Idade", "`Idade` IS NULL OR `Idade` BETWEEN 1 AND 120");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "RegistrosGlicemia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Hora = table.Column<TimeSpan>(type: "time(0)", precision: 0, nullable: false),
                    Glicemia = table.Column<int>(type: "int", nullable: true),
                    GlicemiaAcimaDoLimite = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Dose = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    Refeicao = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosGlicemia", x => x.Id);
                    table.CheckConstraint("CK_Registros_Dose", "`Dose` BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Registros_GlicemiaFaixa", "`Glicemia` IS NULL OR `Glicemia` BETWEEN 20 AND 600");
                    table.CheckConstraint("CK_Registros_HI", "(`GlicemiaAcimaDoLimite` = 1 AND `Glicemia` IS NULL) OR (`GlicemiaAcimaDoLimite` = 0 AND `Glicemia` IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_RegistrosGlicemia_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosGlicemia_UsuarioId_Data_Hora",
                table: "RegistrosGlicemia",
                columns: new[] { "UsuarioId", "Data", "Hora" });

            migrationBuilder.CreateIndex(
                name: "UX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosGlicemia");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
