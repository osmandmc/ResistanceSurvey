using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class ResistamceReasonName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProtestoReason_Resistance_ResistanceId",
                table: "ProtestoReason");

            migrationBuilder.DropForeignKey(
                name: "FK_ResistanceResistanceReason_ProtestoReason_ResistanceReasonId",
                table: "ResistanceResistanceReason");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProtestoReason",
                table: "ProtestoReason");

            migrationBuilder.RenameTable(
                name: "ProtestoReason",
                newName: "ResistanceReason");

            migrationBuilder.RenameIndex(
                name: "IX_ProtestoReason_ResistanceId",
                table: "ResistanceReason",
                newName: "IX_ResistanceReason_ResistanceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResistanceReason",
                table: "ResistanceReason",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResistanceReason_Resistance_ResistanceId",
                table: "ResistanceReason",
                column: "ResistanceId",
                principalTable: "Resistance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResistanceResistanceReason_ResistanceReason_ResistanceReasonId",
                table: "ResistanceResistanceReason",
                column: "ResistanceReasonId",
                principalTable: "ResistanceReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResistanceReason_Resistance_ResistanceId",
                table: "ResistanceReason");

            migrationBuilder.DropForeignKey(
                name: "FK_ResistanceResistanceReason_ResistanceReason_ResistanceReasonId",
                table: "ResistanceResistanceReason");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResistanceReason",
                table: "ResistanceReason");

            migrationBuilder.RenameTable(
                name: "ResistanceReason",
                newName: "ProtestoReason");

            migrationBuilder.RenameIndex(
                name: "IX_ResistanceReason_ResistanceId",
                table: "ProtestoReason",
                newName: "IX_ProtestoReason_ResistanceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProtestoReason",
                table: "ProtestoReason",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProtestoReason_Resistance_ResistanceId",
                table: "ProtestoReason",
                column: "ResistanceId",
                principalTable: "Resistance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ResistanceResistanceReason_ProtestoReason_ResistanceReasonId",
                table: "ResistanceResistanceReason",
                column: "ResistanceReasonId",
                principalTable: "ProtestoReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
