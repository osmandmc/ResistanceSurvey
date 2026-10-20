using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <inheritdoc />
    public partial class TurkishCollation : Migration
    {
        // SQL Server will not change a column's collation while statistics use it. The ones it created by itself
        // (_WA_Sys_…, e.g. on Company.Name) are dropped first; it creates them again when queries need them.
        private const string DropAutomaticStatistics = """
            DECLARE @sql nvarchar(max) = N'';
            SELECT @sql += N'DROP STATISTICS ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N'.' + QUOTENAME(s.name) + N';'
            FROM sys.stats s
            JOIN sys.stats_columns sc ON sc.object_id = s.object_id AND sc.stats_id = s.stats_id
            JOIN sys.columns c ON c.object_id = sc.object_id AND c.column_id = sc.column_id
            JOIN sys.tables t ON t.object_id = s.object_id
            WHERE s.auto_created = 1
              AND ((t.name = 'Resistance' AND c.name IN ('Name', 'Description'))
                OR (t.name IN ('Company', 'Corporation') AND c.name = 'Name')
                OR (t.name = 'News' AND c.name = 'Header'));
            EXEC sp_executesql @sql;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DropAutomaticStatistics);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Resistance",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Turkish_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Production has this column (as "description"), but no migration ever created it: only alter it where it exists.
            migrationBuilder.Sql("IF COL_LENGTH('dbo.Resistance', 'Description') IS NOT NULL ALTER TABLE [Resistance] ALTER COLUMN [Description] nvarchar(max) COLLATE Turkish_CI_AS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Header",
                table: "News",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Turkish_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Corporation",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Turkish_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Company",
                type: "nvarchar(max)",
                nullable: true,
                collation: "Turkish_CI_AS",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DropAutomaticStatistics);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Resistance",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Turkish_CI_AS");

            migrationBuilder.Sql("IF COL_LENGTH('dbo.Resistance', 'Description') IS NOT NULL ALTER TABLE [Resistance] ALTER COLUMN [Description] nvarchar(max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "Header",
                table: "News",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Turkish_CI_AS");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Corporation",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Turkish_CI_AS");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Company",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldCollation: "Turkish_CI_AS");
        }
    }
}
