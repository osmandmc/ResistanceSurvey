using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class ResistamceReasonfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResistanceReason_Resistance_ResistanceId",
                table: "ResistanceReason");

            migrationBuilder.DropIndex(
                name: "IX_ResistanceReason_ResistanceId",
                table: "ResistanceReason");

            migrationBuilder.DropColumn(
                name: "ResistanceId",
                table: "ResistanceReason");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResistanceId",
                table: "ResistanceReason",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceReason_ResistanceId",
                table: "ResistanceReason",
                column: "ResistanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResistanceReason_Resistance_ResistanceId",
                table: "ResistanceReason",
                column: "ResistanceId",
                principalTable: "Resistance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
