using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class resistanceNews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResistanceNews",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ResistanceId = table.Column<int>(nullable: false),
                    NewsId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResistanceNews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResistanceNews_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResistanceNews_Resistance_ResistanceId",
                        column: x => x.ResistanceId,
                        principalTable: "Resistance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceNews_NewsId",
                table: "ResistanceNews",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceNews_ResistanceId",
                table: "ResistanceNews",
                column: "ResistanceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResistanceNews");
        }
    }
}
