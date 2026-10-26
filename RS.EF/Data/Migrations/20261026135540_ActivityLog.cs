using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityEntry",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, collation: "Turkish_CI_AS"),
                    Action = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    CaseId = table.Column<int>(type: "int", nullable: true),
                    ArticleId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    Subject = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: true),
                    SubjectId = table.Column<int>(type: "int", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, collation: "Turkish_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityEntry", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityEntry_At",
                table: "ActivityEntry",
                column: "At");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityEntry_CaseId",
                table: "ActivityEntry",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityEntry_Username_At",
                table: "ActivityEntry",
                columns: new[] { "Username", "At" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityEntry");
        }
    }
}
