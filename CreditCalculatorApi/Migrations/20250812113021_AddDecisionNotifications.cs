using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditCalculatorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDecisionNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DecisionNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionNotifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DecisionNotifications_ApplicationId_Decision",
                table: "DecisionNotifications",
                columns: new[] { "ApplicationId", "Decision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisionNotifications");
        }
    }
}
