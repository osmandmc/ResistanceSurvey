using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// “Kurum türü” (R37) left the forms on 2026-11-06 (İstihdam türü already says it). The test database held 3 values,
    /// all test entries; a holding is recorded as a company relation (İlişki türü: grubun parçası) instead. Down brings
    /// the column back empty.
    /// </summary>
    public partial class DropInstitutionKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstitutionKind",
                table: "Company");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstitutionKind",
                table: "Company",
                type: "int",
                nullable: true);
        }
    }
}
