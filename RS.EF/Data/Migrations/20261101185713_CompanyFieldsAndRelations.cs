using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// R37, parts B and C: facts between companies (CompanyRelation); “Kurum türü” (InstitutionKind); the işkolu list split
    /// so civil servants' hizmet kolları (“MEMUR - …”) go to ServiceBranchId; the ownership values (CompanyType, shown as
    /// “Sahiplik”) renamed. The old subcontractor links (CompanyOutsourceCompany) stay in the database untouched.
    /// </summary>
    public partial class CompanyFieldsAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCivilService",
                table: "CompanyWorkLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "InstitutionKind",
                table: "Company",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceBranchId",
                table: "Company",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompanyRelation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    RelatedCompanyId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    SharePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Turkish_CI_AS"),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Turkish_CI_AS"),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, collation: "Turkish_CI_AS"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyRelation_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyRelation_Company_RelatedCompanyId",
                        column: x => x.RelatedCompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Company_ServiceBranchId",
                table: "Company",
                column: "ServiceBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRelation_CompanyId",
                table: "CompanyRelation",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRelation_RelatedCompanyId",
                table: "CompanyRelation",
                column: "RelatedCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_CompanyWorkLine_ServiceBranchId",
                table: "Company",
                column: "ServiceBranchId",
                principalTable: "CompanyWorkLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // The hizmet kolları are the list's “MEMUR - …” values; a company that has one gets it as its hizmet kolu.
            migrationBuilder.Sql(@"
UPDATE [CompanyWorkLine] SET [IsCivilService] = 1 WHERE LTRIM([Name]) LIKE N'MEMUR%';
UPDATE c SET [ServiceBranchId] = c.[CompanyWorkLineId], [CompanyWorkLineId] = NULL
FROM [Company] c JOIN [CompanyWorkLine] w ON w.[Id] = c.[CompanyWorkLineId]
WHERE w.[IsCivilService] = 1;");

            // Sahiplik: the old type names (checked on production 2026-11-01), renamed only where they still read so.
            migrationBuilder.Sql(@"
UPDATE [CompanyType] SET [Name] = N'Özel yerli' WHERE [Id] = 2 AND LTRIM(RTRIM([Name])) = N'Yerli';
UPDATE [CompanyType] SET [Name] = N'Karma (yerli ve yabancı)' WHERE [Id] = 3 AND LTRIM(RTRIM([Name])) = N'Yerli & Yabanci';
UPDATE [CompanyType] SET [Name] = N'Özel yabancı' WHERE [Id] = 4 AND LTRIM(RTRIM([Name])) = N'Yabanci';
UPDATE [CompanyType] SET [Name] = N'Diğer' WHERE [Id] = 5 AND LTRIM(RTRIM([Name])) = N'Diger';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [Company] SET [CompanyWorkLineId] = COALESCE([CompanyWorkLineId], [ServiceBranchId]);
UPDATE [CompanyType] SET [Name] = N'Yerli ' WHERE [Id] = 2 AND [Name] = N'Özel yerli';
UPDATE [CompanyType] SET [Name] = N'Yerli & Yabanci' WHERE [Id] = 3 AND [Name] = N'Karma (yerli ve yabancı)';
UPDATE [CompanyType] SET [Name] = N'Yabanci' WHERE [Id] = 4 AND [Name] = N'Özel yabancı';
UPDATE [CompanyType] SET [Name] = N'Diger' WHERE [Id] = 5 AND [Name] = N'Diğer';");

            migrationBuilder.DropForeignKey(
                name: "FK_Company_CompanyWorkLine_ServiceBranchId",
                table: "Company");

            migrationBuilder.DropTable(
                name: "CompanyRelation");

            migrationBuilder.DropIndex(
                name: "IX_Company_ServiceBranchId",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "IsCivilService",
                table: "CompanyWorkLine");

            migrationBuilder.DropColumn(
                name: "InstitutionKind",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "ServiceBranchId",
                table: "Company");
        }
    }
}
