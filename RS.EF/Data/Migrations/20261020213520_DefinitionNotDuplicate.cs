using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <inheritdoc />
    public partial class DefinitionNotDuplicate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefinitionNotDuplicate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, collation: "Turkish_CI_AS"),
                    FirstId = table.Column<int>(type: "int", nullable: false),
                    SecondId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefinitionNotDuplicate", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefinitionNotDuplicate_Kind_FirstId_SecondId",
                table: "DefinitionNotDuplicate",
                columns: new[] { "Kind", "FirstId", "SecondId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefinitionNotDuplicate");
        }
    }
}
