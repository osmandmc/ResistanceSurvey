using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// N2: SchemaFixes drops the old TradeUnion table. No case uses it, but a few of its union names exist nowhere else
    /// (2026-10-21: "TÜRK-İŞ Maden-İŞ", "KESK Birleşik-Taşımacılık-Sen", "HAK-İş Öz Gıda-İş", "Tüm Emek Sen",
    /// "Hak-İş Hizmet-İş"). They are copied into Corporation as unions, unapproved, so they wait for review under
    /// Tanımlar → Kurumlar ("onay bekliyor"); near-duplicates show under "Olası tekrarlar" there.
    /// Timestamped one second before SchemaFixes so production runs it first; where the table is already gone (the
    /// test environment) it does nothing. Names Corporation already has (same name, Turkish rules, case ignored) and
    /// names of one character are skipped.
    /// </summary>
    [DbContext(typeof(RSDBContext))]
    [Migration("20261020222826_KeepTradeUnionNames")]
    public partial class KeepTradeUnionNames : Migration
    {
        /// <summary>Corporation type "Sendika" (RS.COMMON KnownListIds.TradeUnionCorporationType).</summary>
        private const int TradeUnionCorporationType = 1;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
IF OBJECT_ID(N'[dbo].[TradeUnion]', N'U') IS NOT NULL
    EXEC(N'
        INSERT INTO [dbo].[Corporation] ([Name], [Approved], [CorporationTypeId])
        SELECT DISTINCT LTRIM(RTRIM(t.[Name])), 0, {TradeUnionCorporationType}
        FROM [dbo].[TradeUnion] t
        WHERE LEN(LTRIM(RTRIM(t.[Name]))) > 1
          AND NOT EXISTS (
              SELECT 1 FROM [dbo].[Corporation] c
              WHERE LTRIM(RTRIM(c.[Name])) COLLATE Turkish_CI_AS = LTRIM(RTRIM(t.[Name])) COLLATE Turkish_CI_AS);');");
        }

        /// <summary>The copied names stay: they may be in use by then.</summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
