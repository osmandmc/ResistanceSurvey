using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <inheritdoc />
    public partial class TargetType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resistance_Company_CompanyId",
                table: "Resistance");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "Resistance",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CorporationId",
                table: "Resistance",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResistanceTargetType",
                table: "Resistance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Resistance_CorporationId",
                table: "Resistance",
                column: "CorporationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resistance_Company_CompanyId",
                table: "Resistance",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Resistance_Corporation_CorporationId",
                table: "Resistance",
                column: "CorporationId",
                principalTable: "Corporation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resistance_Company_CompanyId",
                table: "Resistance");

            migrationBuilder.DropForeignKey(
                name: "FK_Resistance_Corporation_CorporationId",
                table: "Resistance");

            migrationBuilder.DropIndex(
                name: "IX_Resistance_CorporationId",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "CorporationId",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "ResistanceTargetType",
                table: "Resistance");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "Resistance",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Resistance_Company_CompanyId",
                table: "Resistance",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
