using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FieldChange",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, collation: "Turkish_CI_AS"),
                    Source = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ActivityEntryId = table.Column<long>(type: "bigint", nullable: true),
                    Entity = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    RecordId = table.Column<int>(type: "int", nullable: false),
                    CaseId = table.Column<int>(type: "int", nullable: true),
                    Field = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Turkish_CI_AS"),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Turkish_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldChange_ActivityEntry_ActivityEntryId",
                        column: x => x.ActivityEntryId,
                        principalTable: "ActivityEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldChange_ActivityEntryId",
                table: "FieldChange",
                column: "ActivityEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldChange_CaseId",
                table: "FieldChange",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldChange_Entity_RecordId",
                table: "FieldChange",
                columns: new[] { "Entity", "RecordId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldChange");
        }
    }
}
