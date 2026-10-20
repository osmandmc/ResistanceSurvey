using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <inheritdoc />
    public partial class TurkishCollationAll : Migration
    {
        // Production's column types differ from the model in places (e.g. ProtestoLocation.Place is nchar(1000)), so
        // instead of EF's AlterColumn (which would also change types) this reads each Unicode text column's actual type,
        // length and nullability and changes only its collation. Skipped: EF's history table, varchar columns (ArticleId,
        // a hex hash) and, going down, the five columns the TurkishCollation migration changed.
        private static string SetCollation(string collation, string skip = "") => $$"""
            DECLARE @sql nvarchar(max) = N'';

            -- SQL Server will not change a column's collation while statistics use it; the automatic ones go first.
            SELECT @sql += N'DROP STATISTICS ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N'.' + QUOTENAME(s.name) + N';'
            FROM sys.stats s
            JOIN sys.stats_columns sc ON sc.object_id = s.object_id AND sc.stats_id = s.stats_id
            JOIN sys.columns c ON c.object_id = sc.object_id AND c.column_id = sc.column_id
            JOIN sys.tables t ON t.object_id = s.object_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            WHERE s.auto_created = 1 AND t.is_ms_shipped = 0 AND t.name <> '__EFMigrationsHistory'
              AND ty.name IN ('nchar', 'nvarchar') AND c.collation_name <> '{{collation}}'{{skip}};

            SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
                + N' ALTER COLUMN ' + QUOTENAME(c.name) + N' ' + ty.name
                + CASE WHEN c.max_length = -1 THEN N'(max)' ELSE N'(' + CAST(c.max_length / 2 AS nvarchar(10)) + N')' END
                + N' COLLATE {{collation}}' + CASE WHEN c.is_nullable = 1 THEN N' NULL;' ELSE N' NOT NULL;' END
            FROM sys.columns c
            JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            WHERE t.is_ms_shipped = 0 AND t.name <> '__EFMigrationsHistory'
              AND ty.name IN ('nchar', 'nvarchar') AND c.collation_name <> '{{collation}}'{{skip}};

            EXEC sp_executesql @sql;
            """;

        // Changed by the earlier TurkishCollation migration; they stay Turkish when this one is reverted.
        private const string SkipEarlierColumns = """

              AND NOT ((t.name = 'Resistance' AND c.name IN ('Name', 'Description'))
                OR (t.name IN ('Company', 'Corporation') AND c.name = 'Name')
                OR (t.name = 'News' AND c.name = 'Header'))
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(SetCollation("Turkish_CI_AS"));

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(SetCollation("SQL_Latin1_General_CP1_CI_AS", SkipEarlierColumns));
    }
}
