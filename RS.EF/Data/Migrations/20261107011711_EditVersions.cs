using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// Edit conflicts (IVersioned): a version on cases and protests, checked on every save, so two researchers editing the
    /// same case cannot overwrite each other's changes without knowing. Existing rows start at 0.
    /// </summary>
    public partial class EditVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Resistance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Protesto",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Protesto");
        }
    }
}
