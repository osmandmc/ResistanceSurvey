using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class resistanceNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Resistance",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Resistance");
        }
    }
}
