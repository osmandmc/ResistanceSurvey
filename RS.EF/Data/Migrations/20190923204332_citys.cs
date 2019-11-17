using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RS.EF.Data.Migrations
{
    public partial class citys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProtestoCity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProtestoId = table.Column<int>(nullable: false),
                    CityId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoCity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtestoCity_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtestoCity_Protesto_ProtestoId",
                        column: x => x.ProtestoId,
                        principalTable: "Protesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoDistrict",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProtestoId = table.Column<int>(nullable: false),
                    DistrictId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoDistrict", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtestoDistrict_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtestoDistrict_Protesto_ProtestoId",
                        column: x => x.ProtestoId,
                        principalTable: "Protesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoCity_CityId",
                table: "ProtestoCity",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoCity_ProtestoId",
                table: "ProtestoCity",
                column: "ProtestoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoDistrict_DistrictId",
                table: "ProtestoDistrict",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoDistrict_ProtestoId",
                table: "ProtestoDistrict",
                column: "ProtestoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProtestoCity");

            migrationBuilder.DropTable(
                name: "ProtestoDistrict");
        }
    }
}
