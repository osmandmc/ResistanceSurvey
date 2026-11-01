using Microsoft.EntityFrameworkCore.Migrations;
using RS.COMMON.Constants;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// R37, part A: workplaces as places, and how the employer works in each case. Existing data is only read: CompanyId
    /// (the employer) and MainCompanyId (the organization above it) stay as they are, so Down only drops what Up adds.
    /// </summary>
    public partial class EmployerAndWorkplace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployerRole",
                table: "Resistance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EmployerUnknown",
                table: "Resistance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WorkplaceId",
                table: "Resistance",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Workplace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    OwnerCompanyId = table.Column<int>(type: "int", nullable: true),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Turkish_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workplace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workplace_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workplace_Company_OwnerCompanyId",
                        column: x => x.OwnerCompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workplace_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resistance_WorkplaceId",
                table: "Resistance",
                column: "WorkplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_CityId",
                table: "Workplace",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_DistrictId",
                table: "Workplace",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_Name",
                table: "Workplace",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_OwnerCompanyId",
                table: "Workplace",
                column: "OwnerCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resistance_Workplace_WorkplaceId",
                table: "Resistance",
                column: "WorkplaceId",
                principalTable: "Workplace",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // İşletme cases (deleted ones too, so a restored case is consistent): an employer without an organization above
            // it is a direct employer. With one above it, the role is a guess: TOKİ or Emlak Konut gave out a whole job
            // (Yüklenici), anything else is taken as subcontracting (Taşeron). Each guess is recorded, unresolved, for the
            // researchers to check: some are a holding above its own company (Cengiz Holding → Eti Bakır), which is not
            // subcontracting. Cases without an employer, and Genel and Dayanışma cases, stay not set (0).
            migrationBuilder.Sql($@"
UPDATE [Resistance] SET [EmployerRole] = {(int)EmployerRoleValues.Direct}
WHERE [CategoryId] = {KnownListIds.WorkplaceCategory} AND [CompanyId] IS NOT NULL AND [MainCompanyId] IS NULL;

UPDATE r SET [EmployerRole] = CASE
    WHEN LTRIM(mc.[Name]) LIKE N'TOKİ%' OR LTRIM(mc.[Name]) LIKE N'TOKI %' OR LTRIM(RTRIM(mc.[Name])) = N'TOKI'
      OR mc.[Name] LIKE N'%Toplu Konut İdaresi%' OR mc.[Name] LIKE N'%Emlak Konut%'
    THEN {(int)EmployerRoleValues.Contractor} ELSE {(int)EmployerRoleValues.Subcontractor} END
FROM [Resistance] r JOIN [Company] mc ON mc.[Id] = r.[MainCompanyId]
WHERE r.[CategoryId] = {KnownListIds.WorkplaceCategory};

INSERT INTO [CaseMigrationRecord] ([ResistanceId], [Step], [OldCompanyId], [OldCompanyName], [OldMainCompanyId], [OldCorporationId],
    [OldTargetType], [OldName], [RemovedCorporationIds], [AddedOrganizingLink], [CompanyMarkedDeleted], [ResolvedAt], [At])
SELECT r.[Id], 'employer-role', r.[CompanyId], c.[Name], r.[MainCompanyId], NULL, 0, r.[Name], NULL, 0, 0, NULL, SYSUTCDATETIME()
FROM [Resistance] r LEFT JOIN [Company] c ON c.[Id] = r.[CompanyId]
WHERE r.[CategoryId] = {KnownListIds.WorkplaceCategory} AND r.[MainCompanyId] IS NOT NULL;");
        }

        /// <summary>The role values as the migration wrote them (fixed here, whatever the enum becomes later).</summary>
        private static class EmployerRoleValues
        {
            public const int Direct = 1;
            public const int Subcontractor = 2;
            public const int Contractor = 3;
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [CaseMigrationRecord] WHERE [Step] = 'employer-role';");

            migrationBuilder.DropForeignKey(
                name: "FK_Resistance_Workplace_WorkplaceId",
                table: "Resistance");

            migrationBuilder.DropTable(
                name: "Workplace");

            migrationBuilder.DropIndex(
                name: "IX_Resistance_WorkplaceId",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "EmployerRole",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "EmployerUnknown",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "WorkplaceId",
                table: "Resistance");
        }
    }
}
