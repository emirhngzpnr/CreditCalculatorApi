using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditCalculatorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNumberToCreditApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserNumber",
                table: "CreditApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserNumber",
                table: "CreditApplications");
        }
    }
}
