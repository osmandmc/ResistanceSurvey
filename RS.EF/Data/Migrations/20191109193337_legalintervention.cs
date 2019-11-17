using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class legalintervention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnyLegalIntervention",
                table: "Protesto");

            migrationBuilder.DropColumn(
                name: "FiredEmployeeCountByProtesto",
                table: "Protesto");

            migrationBuilder.DropColumn(
                name: "LegalInterventionDesc",
                table: "Protesto");

            migrationBuilder.AddColumn<bool>(
                name: "AnyLegalIntervention",
                table: "Resistance",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FiredEmployeeCountByProtesto",
                table: "Resistance",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalInterventionDesc",
                table: "Resistance",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnyLegalIntervention",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "FiredEmployeeCountByProtesto",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "LegalInterventionDesc",
                table: "Resistance");

            migrationBuilder.AddColumn<bool>(
                name: "AnyLegalIntervention",
                table: "Protesto",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FiredEmployeeCountByProtesto",
                table: "Protesto",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalInterventionDesc",
                table: "Protesto",
                nullable: true);
        }
    }
}
