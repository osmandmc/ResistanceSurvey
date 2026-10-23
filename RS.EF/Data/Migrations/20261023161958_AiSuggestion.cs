using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <inheritdoc />
    public partial class AiSuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiSuggestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    DismissKind = table.Column<int>(type: "int", nullable: true),
                    ResistanceId = table.Column<int>(type: "int", nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, collation: "Turkish_CI_AS"),
                    PromptVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, collation: "Turkish_CI_AS"),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Turkish_CI_AS"),
                    Relevant = table.Column<bool>(type: "bit", nullable: false),
                    Applied = table.Column<bool>(type: "bit", nullable: false),
                    Suggested = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Turkish_CI_AS"),
                    Saved = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Turkish_CI_AS"),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, collation: "Turkish_CI_AS"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiSuggestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiSuggestion_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AiSuggestion_Resistance_ResistanceId",
                        column: x => x.ResistanceId,
                        principalTable: "Resistance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiSuggestion_NewsId",
                table: "AiSuggestion",
                column: "NewsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiSuggestion_ResistanceId",
                table: "AiSuggestion",
                column: "ResistanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiSuggestion");
        }
    }
}
