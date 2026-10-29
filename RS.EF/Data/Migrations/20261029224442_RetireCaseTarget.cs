using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// R31d: "Kime karşı" (ResistanceTargetType) and "Hedef kurum" (CorporationId) go; nothing reads them since R31b.
    /// Their values are kept in CaseMigrationRecord (CaseCategories recorded every case that had a target institution),
    /// and Down puts them back from there.
    /// </summary>
    public partial class RetireCaseTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_Resistance_Corporation_CorporationId",
                table: "Resistance",
                column: "CorporationId",
                principalTable: "Corporation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Every case targeted a company unless the record says otherwise.
            migrationBuilder.Sql(@"
UPDATE [Resistance] SET [ResistanceTargetType] = 1;
UPDATE r SET [ResistanceTargetType] = m.[OldTargetType]
FROM [Resistance] r JOIN [CaseMigrationRecord] m ON m.[ResistanceId] = r.[Id];
UPDATE r SET [CorporationId] = m.[OldCorporationId]
FROM [Resistance] r JOIN [CaseMigrationRecord] m ON m.[ResistanceId] = r.[Id] AND m.[Step] = 'workplace-target';");
        }
    }
}
