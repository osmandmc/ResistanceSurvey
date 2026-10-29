using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// R31: the case category decides the form. Adds the organizers' role, the supported case of a Dayanışma case and the
    /// migration record, then moves Genel and Dayanışma cases to the new model. Every change is written to
    /// CaseMigrationRecord first, so it can be checked or undone (Down restores it). EF runs a migration in one transaction:
    /// a failure leaves the database as it was. Category ids: RS.COMMON KnownListIds (1 İşletme/Kurum, 2 Dayanışma, 3 Genel).
    /// </summary>
    public partial class CaseCategories : Migration
    {
        private const int Solidarity = 2;
        private const int General = 3;

        // Placeholder organizers that mean "no organization": in Genel and Dayanışma an empty list now says so.
        private const string Placeholders = "N'Kurumsuz', N'Diger', N'Diğer'";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Every existing link keeps today's meaning: a supporting organization.
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "ResistanceCorporation",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "SupportedResistanceId",
                table: "Resistance",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CaseMigrationRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResistanceId = table.Column<int>(type: "int", nullable: false),
                    Step = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    OldCompanyId = table.Column<int>(type: "int", nullable: true),
                    OldCompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Turkish_CI_AS"),
                    OldMainCompanyId = table.Column<int>(type: "int", nullable: true),
                    OldCorporationId = table.Column<int>(type: "int", nullable: true),
                    OldTargetType = table.Column<int>(type: "int", nullable: false),
                    OldName = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Turkish_CI_AS"),
                    RemovedCorporationIds = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    AddedOrganizingLink = table.Column<bool>(type: "bit", nullable: false),
                    CompanyMarkedDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseMigrationRecord", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resistance_SupportedResistanceId",
                table: "Resistance",
                column: "SupportedResistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseMigrationRecord_ResistanceId",
                table: "CaseMigrationRecord",
                column: "ResistanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resistance_Resistance_SupportedResistanceId",
                table: "Resistance",
                column: "SupportedResistanceId",
                principalTable: "Resistance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // The categories must be the ones the ids mean (a database built from the migrations has none: nothing to move).
            migrationBuilder.Sql($@"
IF EXISTS (SELECT 1 FROM [Category])
   AND NOT EXISTS (SELECT 1 FROM [Category] WHERE [Id] = {Solidarity} AND [Name] LIKE N'Dayan%')
    THROW 50001, 'CaseCategories: category {Solidarity} is not Dayanışma; check KnownListIds before migrating.', 1;
IF EXISTS (SELECT 1 FROM [Category])
   AND NOT EXISTS (SELECT 1 FROM [Category] WHERE [Id] = {General} AND [Name] LIKE N'Genel%')
    THROW 50002, 'CaseCategories: category {General} is not Genel; check KnownListIds before migrating.', 1;");

            // 1. The record, before anything changes: Genel and Dayanışma cases, and İşletme cases with a "Hedef kurum".
            migrationBuilder.Sql($@"
INSERT INTO [CaseMigrationRecord] ([ResistanceId], [Step], [OldCompanyId], [OldCompanyName], [OldMainCompanyId], [OldCorporationId],
    [OldTargetType], [OldName], [RemovedCorporationIds], [AddedOrganizingLink], [CompanyMarkedDeleted], [At])
SELECT r.[Id],
       CASE r.[CategoryId] WHEN {General} THEN 'general' WHEN {Solidarity} THEN 'solidarity' ELSE 'workplace-target' END,
       r.[CompanyId], c.[Name], r.[MainCompanyId], r.[CorporationId], r.[ResistanceTargetType], r.[Name],
       STUFF((SELECT ',' + CAST(rc.[CorporationId] AS varchar(12)) FROM [ResistanceCorporation] rc
            JOIN [Corporation] co ON co.[Id] = rc.[CorporationId]
         WHERE rc.[ResistanceId] = r.[Id] AND r.[CategoryId] IN ({Solidarity}, {General}) AND LTRIM(RTRIM(co.[Name])) IN ({Placeholders})
         FOR XML PATH('')), 1, 1, ''),
       0, 0, SYSUTCDATETIME()
FROM [Resistance] r
LEFT JOIN [Company] c ON c.[Id] = r.[CompanyId]
WHERE r.[CategoryId] IN ({Solidarity}, {General}) OR r.[CorporationId] IS NOT NULL;");

            // 2. Unnamed Genel and Dayanışma cases take their old company name: the company field served as their title.
            migrationBuilder.Sql($@"
UPDATE r SET [Name] = LTRIM(RTRIM(m.[OldCompanyName]))
FROM [Resistance] r JOIN [CaseMigrationRecord] m ON m.[ResistanceId] = r.[Id] AND m.[Step] IN ('general', 'solidarity')
WHERE (r.[Name] IS NULL OR LTRIM(RTRIM(r.[Name])) = N'') AND LTRIM(RTRIM(ISNULL(m.[OldCompanyName], N''))) <> N'';");

            // 3. Their organizations organized the action; the old "Hedef kurum" too.
            migrationBuilder.Sql($@"
UPDATE rc SET [Role] = 2
FROM [ResistanceCorporation] rc JOIN [Resistance] r ON r.[Id] = rc.[ResistanceId]
WHERE r.[CategoryId] IN ({Solidarity}, {General});
UPDATE m SET [AddedOrganizingLink] = 1
FROM [CaseMigrationRecord] m JOIN [Resistance] r ON r.[Id] = m.[ResistanceId]
WHERE m.[Step] IN ('general', 'solidarity') AND r.[CorporationId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [ResistanceCorporation] x WHERE x.[ResistanceId] = r.[Id] AND x.[CorporationId] = r.[CorporationId]);
INSERT INTO [ResistanceCorporation] ([ResistanceId], [CorporationId], [Role])
SELECT r.[Id], r.[CorporationId], 2 FROM [Resistance] r
WHERE r.[CategoryId] IN ({Solidarity}, {General}) AND r.[CorporationId] IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [ResistanceCorporation] x WHERE x.[ResistanceId] = r.[Id] AND x.[CorporationId] = r.[CorporationId]);");

            // 4. Placeholder organizers go (recorded in step 1): an empty list means kurumsuz. İşletme cases keep theirs.
            migrationBuilder.Sql($@"
DELETE rc FROM [ResistanceCorporation] rc
JOIN [Resistance] r ON r.[Id] = rc.[ResistanceId] JOIN [Corporation] co ON co.[Id] = rc.[CorporationId]
WHERE r.[CategoryId] IN ({Solidarity}, {General}) AND LTRIM(RTRIM(co.[Name])) IN ({Placeholders});");

            // 5. Genel and Dayanışma cases have no employer, main company or target institution any more. A Dayanışma case's
            //    old company comes back on the linking screen (R31c) when it was the workplace whose workers acted.
            migrationBuilder.Sql($@"
UPDATE [Resistance] SET [CompanyId] = NULL, [MainCompanyId] = NULL, [CorporationId] = NULL
WHERE [CategoryId] IN ({Solidarity}, {General});");

            // 6. Companies used only by Genel cases were campaign and event names: marked deleted (kept for reference).
            migrationBuilder.Sql($@"
UPDATE c SET [Deleted] = 1
FROM [Company] c
WHERE c.[Deleted] = 0
  AND EXISTS (SELECT 1 FROM [CaseMigrationRecord] m WHERE m.[OldCompanyId] = c.[Id] AND m.[Step] = 'general')
  AND NOT EXISTS (SELECT 1 FROM [CaseMigrationRecord] m WHERE m.[OldCompanyId] = c.[Id] AND m.[Step] <> 'general')
  AND NOT EXISTS (SELECT 1 FROM [Resistance] r WHERE r.[CompanyId] = c.[Id] OR r.[MainCompanyId] = c.[Id])
  AND NOT EXISTS (SELECT 1 FROM [CompanyOutsourceCompany] o WHERE o.[CompanyId] = c.[Id] OR o.[OutsourceCompanyId] = c.[Id]);
UPDATE m SET [CompanyMarkedDeleted] = 1
FROM [CaseMigrationRecord] m JOIN [Company] c ON c.[Id] = m.[OldCompanyId]
WHERE m.[Step] = 'general' AND c.[Deleted] = 1
  AND m.[Id] = (SELECT MIN(x.[Id]) FROM [CaseMigrationRecord] x WHERE x.[OldCompanyId] = m.[OldCompanyId]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Undo from the record: companies, names, target institution, removed placeholders, the organizers' role.
            migrationBuilder.Sql(@"
UPDATE c SET [Deleted] = 0
FROM [Company] c JOIN [CaseMigrationRecord] m ON m.[OldCompanyId] = c.[Id] AND m.[CompanyMarkedDeleted] = 1;
UPDATE r SET [CompanyId] = m.[OldCompanyId], [MainCompanyId] = m.[OldMainCompanyId], [CorporationId] = m.[OldCorporationId], [Name] = m.[OldName]
FROM [Resistance] r JOIN [CaseMigrationRecord] m ON m.[ResistanceId] = r.[Id] AND m.[Step] IN ('general', 'solidarity');
DELETE rc FROM [ResistanceCorporation] rc
JOIN [CaseMigrationRecord] m ON m.[ResistanceId] = rc.[ResistanceId] AND m.[AddedOrganizingLink] = 1 AND m.[OldCorporationId] = rc.[CorporationId];
INSERT INTO [ResistanceCorporation] ([ResistanceId], [CorporationId], [Role])
SELECT m.[ResistanceId], co.[Id], 1
FROM [CaseMigrationRecord] m JOIN [Corporation] co ON ',' + m.[RemovedCorporationIds] + ',' LIKE '%,' + CAST(co.[Id] AS varchar(12)) + ',%'
WHERE m.[RemovedCorporationIds] IS NOT NULL;
UPDATE [ResistanceCorporation] SET [Role] = 1 WHERE [Role] = 2;");

            migrationBuilder.DropForeignKey(
                name: "FK_Resistance_Resistance_SupportedResistanceId",
                table: "Resistance");

            migrationBuilder.DropTable(
                name: "CaseMigrationRecord");

            migrationBuilder.DropIndex(
                name: "IX_Resistance_SupportedResistanceId",
                table: "Resistance");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "ResistanceCorporation");

            migrationBuilder.DropColumn(
                name: "SupportedResistanceId",
                table: "Resistance");
        }
    }
}
