using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class Reason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProtestoProtestoReason");

            migrationBuilder.AddColumn<int>(
                name: "ResistanceId",
                table: "ProtestoReason",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoReason_ResistanceId",
                table: "ProtestoReason",
                column: "ResistanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProtestoReason_Resistance_ResistanceId",
                table: "ProtestoReason",
                column: "ResistanceId",
                principalTable: "Resistance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProtestoReason_Resistance_ResistanceId",
                table: "ProtestoReason");

            migrationBuilder.DropIndex(
                name: "IX_ProtestoReason_ResistanceId",
                table: "ProtestoReason");

            migrationBuilder.DropColumn(
                name: "ResistanceId",
                table: "ProtestoReason");

            migrationBuilder.CreateTable(
                name: "ProtestoProtestoReason",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProtestoId = table.Column<int>(nullable: false),
                    ProtestoReasonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoProtestoReason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtestoProtestoReason_Protesto_ProtestoId",
                        column: x => x.ProtestoId,
                        principalTable: "Protesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtestoProtestoReason_ProtestoReason_ProtestoReasonId",
                        column: x => x.ProtestoReasonId,
                        principalTable: "ProtestoReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoProtestoReason_ProtestoId",
                table: "ProtestoProtestoReason",
                column: "ProtestoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoProtestoReason_ProtestoReasonId",
                table: "ProtestoProtestoReason",
                column: "ProtestoReasonId");
        }
    }
}
