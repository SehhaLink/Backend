using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sehha360.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PatientSummary",
                table: "MedicalDocuments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientSummary",
                table: "MedicalDocuments");
        }
    }
}
