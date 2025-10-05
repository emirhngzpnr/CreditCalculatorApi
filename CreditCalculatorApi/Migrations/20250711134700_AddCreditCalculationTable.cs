using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditCalculatorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCalculationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KrediTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Vade = table.Column<int>(type: "int", nullable: false),
                    FaizOrani = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AylikTaksit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ToplamOdeme = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HesaplamaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCalculations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditCalculations");
        }
    }
}
