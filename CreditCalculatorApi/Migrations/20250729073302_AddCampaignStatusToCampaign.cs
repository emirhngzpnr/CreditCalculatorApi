using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditCalculatorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignStatusToCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampaignStatus",
                table: "Campaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CampaignStatus",
                table: "Campaigns");
        }
    }
}
