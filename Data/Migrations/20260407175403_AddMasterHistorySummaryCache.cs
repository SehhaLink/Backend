using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sehha360.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterHistorySummaryCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MasterHistorySummary",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MasterHistorySummary",
                table: "AspNetUsers");
        }
    }
}
