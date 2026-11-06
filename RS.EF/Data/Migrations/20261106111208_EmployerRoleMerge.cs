using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// Researchers' feedback (2026-11-06): İşveren durumu is only Doğrudan or Taşeron. Yüklenici (3) and İşletmeci (4)
    /// become Taşeron (2); the organization above the employer stays and is now its Asıl işveren (TOKİ above the company
    /// building its project, TKİ above the company running its mine). Each changed case, deleted ones too, is recorded
    /// first with its old role, so Down puts it back.
    /// </summary>
    public partial class EmployerRoleMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OldEmployerRole",
                table: "CaseMigrationRecord",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
INSERT INTO [CaseMigrationRecord] ([ResistanceId], [Step], [OldCompanyId], [OldCompanyName], [OldMainCompanyId], [OldCorporationId],
    [OldTargetType], [OldName], [RemovedCorporationIds], [AddedOrganizingLink], [CompanyMarkedDeleted], [ResolvedAt], [OldEmployerRole], [At])
SELECT r.[Id], 'employer-role-merge', r.[CompanyId], c.[Name], r.[MainCompanyId], NULL, 0, r.[Name], NULL, 0, 0, NULL, r.[EmployerRole], SYSUTCDATETIME()
FROM [Resistance] r LEFT JOIN [Company] c ON c.[Id] = r.[CompanyId]
WHERE r.[EmployerRole] IN (3, 4);

UPDATE [Resistance] SET [EmployerRole] = 2 WHERE [EmployerRole] IN (3, 4);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE r SET [EmployerRole] = m.[OldEmployerRole]
FROM [Resistance] r JOIN [CaseMigrationRecord] m ON m.[ResistanceId] = r.[Id] AND m.[Step] = 'employer-role-merge'
WHERE r.[EmployerRole] = 2;

DELETE FROM [CaseMigrationRecord] WHERE [Step] = 'employer-role-merge';");

            migrationBuilder.DropColumn(
                name: "OldEmployerRole",
                table: "CaseMigrationRecord");
        }
    }
}
