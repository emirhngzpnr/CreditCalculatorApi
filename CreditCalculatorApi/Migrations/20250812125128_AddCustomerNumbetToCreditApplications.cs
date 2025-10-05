using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditCalculatorApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerNumbetToCreditApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerNumber",
                table: "CreditApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerNumber",
                table: "CreditApplications");
        }
    }
}
