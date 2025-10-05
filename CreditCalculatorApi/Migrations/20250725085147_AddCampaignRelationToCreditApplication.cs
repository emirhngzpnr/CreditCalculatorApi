using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditCalculatorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignRelationToCreditApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "CreditApplications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_CampaignId",
                table: "CreditApplications",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditApplications_Campaigns_CampaignId",
                table: "CreditApplications",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditApplications_Campaigns_CampaignId",
                table: "CreditApplications");

            migrationBuilder.DropIndex(
                name: "IX_CreditApplications_CampaignId",
                table: "CreditApplications");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "CreditApplications");
        }
    }
}
