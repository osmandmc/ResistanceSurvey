using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class ResistamceReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResistanceResistanceReason",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ResistanceId = table.Column<int>(nullable: false),
                    ResistanceReasonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResistanceResistanceReason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResistanceResistanceReason_Resistance_ResistanceId",
                        column: x => x.ResistanceId,
                        principalTable: "Resistance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResistanceResistanceReason_ProtestoReason_ResistanceReasonId",
                        column: x => x.ResistanceReasonId,
                        principalTable: "ProtestoReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceResistanceReason_ResistanceId",
                table: "ResistanceResistanceReason",
                column: "ResistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceResistanceReason_ResistanceReasonId",
                table: "ResistanceResistanceReason",
                column: "ResistanceReasonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResistanceResistanceReason");
        }
    }
}
