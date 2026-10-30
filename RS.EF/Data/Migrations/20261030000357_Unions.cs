using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// R34: unions and confederations become classes of the organization table (EF table per hierarchy): Kind 1 for the
    /// type "Sendika", 2 for the new type "Konfederasyon"; a union gets an optional confederation. The confederations
    /// (DİSK, TÜRK-İŞ, HAK-İŞ, KESK, Memur-Sen, Türkiye Kamu-Sen, Birleşik Kamu-İş; kept in Tanımlar from now on) are the
    /// existing organizations of those names, retyped, or new ones where missing. A union's confederation comes from its
    /// name's prefix ("DİSK Genel-İş" → DİSK); names stay as they are.
    /// </summary>
    public partial class Unions : Migration
    {
        // Each confederation with the ways its name is written (also as a union name's first words).
        private const string Aliases = @"
DECLARE @aliases TABLE ([Canonical] nvarchar(50) COLLATE Turkish_CI_AS, [Alias] nvarchar(50) COLLATE Turkish_CI_AS);
INSERT INTO @aliases VALUES
    (N'DİSK', N'DİSK'),
    (N'TÜRK-İŞ', N'TÜRK-İŞ'),
    (N'TÜRK-İŞ', N'TÜRK İŞ'),
    (N'TÜRK-İŞ', N'TÜRKİŞ'),
    (N'HAK-İŞ', N'HAK-İŞ'),
    (N'HAK-İŞ', N'HAK İŞ'),
    (N'HAK-İŞ', N'HAKİŞ'),
    (N'KESK', N'KESK'),
    (N'Memur-Sen', N'MEMUR-SEN'),
    (N'Memur-Sen', N'MEMUR SEN'),
    (N'Memur-Sen', N'MEMURSEN'),
    (N'Türkiye Kamu-Sen', N'TÜRKİYE KAMU-SEN'),
    (N'Türkiye Kamu-Sen', N'TÜRKİYE KAMU SEN'),
    (N'Türkiye Kamu-Sen', N'KAMU-SEN'),
    (N'Birleşik Kamu-İş', N'BİRLEŞİK KAMU-İŞ'),
    (N'Birleşik Kamu-İş', N'BİRLEŞİK KAMU İŞ');";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConfederationId",
                table: "Corporation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "Corporation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Corporation_ConfederationId",
                table: "Corporation",
                column: "ConfederationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Corporation_Corporation_ConfederationId",
                table: "Corporation",
                column: "ConfederationId",
                principalTable: "Corporation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // The type "Konfederasyon" with the id the code knows (KnownListIds.ConfederationCorporationType).
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [CorporationType] WHERE [Id] = 6 AND [Name] NOT LIKE N'Konfederasyon%')
    THROW 50003, 'Unions: corporation type 6 exists with another name; check KnownListIds before migrating.', 1;
IF NOT EXISTS (SELECT 1 FROM [CorporationType] WHERE [Id] = 6)
BEGIN
    SET IDENTITY_INSERT [CorporationType] ON;
    INSERT INTO [CorporationType] ([Id], [Name]) VALUES (6, N'Konfederasyon');
    SET IDENTITY_INSERT [CorporationType] OFF;
END");

            // Unions; then the confederations (existing ones retyped, missing ones created); then each union's
            // confederation from its name's prefix. One batch, so the alias table is shared.
            migrationBuilder.Sql(Aliases + @"
UPDATE [Corporation] SET [Kind] = 1 WHERE [CorporationTypeId] = 1;

UPDATE c SET [Kind] = 2, [CorporationTypeId] = 6
FROM [Corporation] c JOIN @aliases a ON LTRIM(RTRIM(c.[Name])) COLLATE Turkish_CI_AS = a.[Alias];

INSERT INTO [Corporation] ([Name], [Approved], [CorporationTypeId], [Kind])
SELECT DISTINCT a.[Canonical], 1, 6, 2 FROM @aliases a
WHERE NOT EXISTS (SELECT 1 FROM [Corporation] c JOIN @aliases b ON b.[Canonical] = a.[Canonical]
                  WHERE c.[Kind] = 2 AND LTRIM(RTRIM(c.[Name])) COLLATE Turkish_CI_AS = b.[Alias]);

UPDATE u SET [ConfederationId] = (
    SELECT MIN(c.[Id]) FROM [Corporation] c JOIN @aliases b ON b.[Canonical] = a.[Canonical]
    WHERE c.[Kind] = 2 AND LTRIM(RTRIM(c.[Name])) COLLATE Turkish_CI_AS = b.[Alias])
FROM [Corporation] u JOIN @aliases a ON LTRIM(u.[Name]) COLLATE Turkish_CI_AS LIKE a.[Alias] + N' %'
WHERE u.[Kind] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Confederations go back to being unions (as they were typed before); the type stays if anything else uses it.
            migrationBuilder.Sql(@"
UPDATE [Corporation] SET [CorporationTypeId] = 1 WHERE [Kind] = 2;
DELETE FROM [CorporationType] WHERE [Id] = 6 AND NOT EXISTS (SELECT 1 FROM [Corporation] WHERE [CorporationTypeId] = 6);");

            migrationBuilder.DropForeignKey(
                name: "FK_Corporation_Corporation_ConfederationId",
                table: "Corporation");

            migrationBuilder.DropIndex(
                name: "IX_Corporation_ConfederationId",
                table: "Corporation");

            migrationBuilder.DropColumn(
                name: "ConfederationId",
                table: "Corporation");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Corporation");
        }
    }
}
