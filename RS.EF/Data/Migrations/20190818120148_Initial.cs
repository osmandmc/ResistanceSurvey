using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RS.EF.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyScale",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyScale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyWorkLine",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyWorkLine", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Corporation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corporation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeCount",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gender",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gender", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterventionType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoEmployeeCount",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoEmployeeCount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoPlace",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoPlace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoReason",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoReason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeUnionAuthority",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeUnionAuthority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeUnionConfederation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeUnionConfederation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CityId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_District", x => x.Id);
                    table.ForeignKey(
                        name: "FK_District_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyScaleId = table.Column<int>(nullable: false),
                    CompanyTypeId = table.Column<int>(nullable: false),
                    CompanyWorkLineId = table.Column<int>(nullable: false),
                    IsOutsource = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Company_CompanyScale_CompanyScaleId",
                        column: x => x.CompanyScaleId,
                        principalTable: "CompanyScale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_CompanyType_CompanyTypeId",
                        column: x => x.CompanyTypeId,
                        principalTable: "CompanyType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Company_CompanyWorkLine_CompanyWorkLineId",
                        column: x => x.CompanyWorkLineId,
                        principalTable: "CompanyWorkLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeUnion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    TradeUnionConfederationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeUnion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeUnion_TradeUnionConfederation_TradeUnionConfederationId",
                        column: x => x.TradeUnionConfederationId,
                        principalTable: "TradeUnionConfederation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyOutsourceCompany",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyId = table.Column<int>(nullable: false),
                    OutsourceCompanyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyOutsourceCompany", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyOutsourceCompany_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyOutsourceCompany_Company_OutsourceCompanyId",
                        column: x => x.OutsourceCompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resistance",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(nullable: false),
                    Code = table.Column<string>(nullable: true),
                    CompanyId = table.Column<int>(nullable: false),
                    EmployeeCountId = table.Column<int>(nullable: true),
                    EmployeeCountNumber = table.Column<int>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    HasTradeUnion = table.Column<bool>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    TradeUnionAuthorityId = table.Column<int>(nullable: true),
                    TradeUnionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resistance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resistance_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resistance_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resistance_EmployeeCount_EmployeeCountId",
                        column: x => x.EmployeeCountId,
                        principalTable: "EmployeeCount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resistance_TradeUnionAuthority_TradeUnionAuthorityId",
                        column: x => x.TradeUnionAuthorityId,
                        principalTable: "TradeUnionAuthority",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resistance_TradeUnion_TradeUnionId",
                        column: x => x.TradeUnionId,
                        principalTable: "TradeUnion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Protesto",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnyLegalIntervention = table.Column<bool>(nullable: false),
                    CustodyCount = table.Column<int>(nullable: true),
                    DevelopRight = table.Column<bool>(nullable: false),
                    EmployeeCountNumber = table.Column<int>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    FiredEmployeeCountByProtesto = table.Column<int>(nullable: true),
                    GenderId = table.Column<int>(nullable: false),
                    IsAgainstProduction = table.Column<bool>(nullable: false),
                    LegalInterventionDesc = table.Column<string>(nullable: true),
                    Note = table.Column<string>(nullable: true),
                    ProtestoEmployeeCountId = table.Column<int>(nullable: true),
                    ResistanceId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Protesto_ProtestoEmployeeCount_ProtestoEmployeeCountId",
                        column: x => x.ProtestoEmployeeCountId,
                        principalTable: "ProtestoEmployeeCount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Protesto_Resistance_ResistanceId",
                        column: x => x.ResistanceId,
                        principalTable: "Resistance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResistanceCorporation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CorporationId = table.Column<int>(nullable: false),
                    ResistanceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResistanceCorporation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResistanceCorporation_Corporation_CorporationId",
                        column: x => x.CorporationId,
                        principalTable: "Corporation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResistanceCorporation_Resistance_ResistanceId",
                        column: x => x.ResistanceId,
                        principalTable: "Resistance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResistanceEmploymentType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmploymentTypeId = table.Column<int>(nullable: false),
                    ResistanceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResistanceEmploymentType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResistanceEmploymentType_EmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalTable: "EmploymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResistanceEmploymentType_Resistance_ResistanceId",
                        column: x => x.ResistanceId,
                        principalTable: "Resistance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoInterventionType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InterventionTypeId = table.Column<int>(nullable: false),
                    ProtestoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoInterventionType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtestoInterventionType_InterventionType_InterventionTypeId",
                        column: x => x.InterventionTypeId,
                        principalTable: "InterventionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtestoInterventionType_Protesto_ProtestoId",
                        column: x => x.ProtestoId,
                        principalTable: "Protesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoProtestoPlace",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProtestoId = table.Column<int>(nullable: false),
                    ProtestoPlaceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoProtestoPlace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtestoProtestoPlace_Protesto_ProtestoId",
                        column: x => x.ProtestoId,
                        principalTable: "Protesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtestoProtestoPlace_ProtestoPlace_ProtestoPlaceId",
                        column: x => x.ProtestoPlaceId,
                        principalTable: "ProtestoPlace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoProtestoReason",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProtestoId = table.Column<int>(nullable: false),
                    ProtestoReasonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoProtestoReason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtestoProtestoReason_Protesto_ProtestoId",
                        column: x => x.ProtestoId,
                        principalTable: "Protesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtestoProtestoReason_ProtestoReason_ProtestoReasonId",
                        column: x => x.ProtestoReasonId,
                        principalTable: "ProtestoReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoProtestoType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProtestoId = table.Column<int>(nullable: false),
                    ProtestoTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoProtestoType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtestoProtestoType_Protesto_ProtestoId",
                        column: x => x.ProtestoId,
                        principalTable: "Protesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtestoProtestoType_ProtestoType_ProtestoTypeId",
                        column: x => x.ProtestoTypeId,
                        principalTable: "ProtestoType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Company_CompanyScaleId",
                table: "Company",
                column: "CompanyScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_CompanyTypeId",
                table: "Company",
                column: "CompanyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Company_CompanyWorkLineId",
                table: "Company",
                column: "CompanyWorkLineId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyOutsourceCompany_CompanyId",
                table: "CompanyOutsourceCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyOutsourceCompany_OutsourceCompanyId",
                table: "CompanyOutsourceCompany",
                column: "OutsourceCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_District_CityId",
                table: "District",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Protesto_ProtestoEmployeeCountId",
                table: "Protesto",
                column: "ProtestoEmployeeCountId");

            migrationBuilder.CreateIndex(
                name: "IX_Protesto_ResistanceId",
                table: "Protesto",
                column: "ResistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoInterventionType_InterventionTypeId",
                table: "ProtestoInterventionType",
                column: "InterventionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoInterventionType_ProtestoId",
                table: "ProtestoInterventionType",
                column: "ProtestoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoProtestoPlace_ProtestoId",
                table: "ProtestoProtestoPlace",
                column: "ProtestoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoProtestoPlace_ProtestoPlaceId",
                table: "ProtestoProtestoPlace",
                column: "ProtestoPlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoProtestoReason_ProtestoId",
                table: "ProtestoProtestoReason",
                column: "ProtestoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoProtestoReason_ProtestoReasonId",
                table: "ProtestoProtestoReason",
                column: "ProtestoReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoProtestoType_ProtestoId",
                table: "ProtestoProtestoType",
                column: "ProtestoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoProtestoType_ProtestoTypeId",
                table: "ProtestoProtestoType",
                column: "ProtestoTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Resistance_CategoryId",
                table: "Resistance",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Resistance_CompanyId",
                table: "Resistance",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Resistance_EmployeeCountId",
                table: "Resistance",
                column: "EmployeeCountId");

            migrationBuilder.CreateIndex(
                name: "IX_Resistance_TradeUnionAuthorityId",
                table: "Resistance",
                column: "TradeUnionAuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Resistance_TradeUnionId",
                table: "Resistance",
                column: "TradeUnionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceCorporation_CorporationId",
                table: "ResistanceCorporation",
                column: "CorporationId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceCorporation_ResistanceId",
                table: "ResistanceCorporation",
                column: "ResistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceEmploymentType_EmploymentTypeId",
                table: "ResistanceEmploymentType",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceEmploymentType_ResistanceId",
                table: "ResistanceEmploymentType",
                column: "ResistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeUnion_TradeUnionConfederationId",
                table: "TradeUnion",
                column: "TradeUnionConfederationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyOutsourceCompany");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "Gender");

            migrationBuilder.DropTable(
                name: "ProtestoInterventionType");

            migrationBuilder.DropTable(
                name: "ProtestoProtestoPlace");

            migrationBuilder.DropTable(
                name: "ProtestoProtestoReason");

            migrationBuilder.DropTable(
                name: "ProtestoProtestoType");

            migrationBuilder.DropTable(
                name: "ResistanceCorporation");

            migrationBuilder.DropTable(
                name: "ResistanceEmploymentType");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "InterventionType");

            migrationBuilder.DropTable(
                name: "ProtestoPlace");

            migrationBuilder.DropTable(
                name: "ProtestoReason");

            migrationBuilder.DropTable(
                name: "Protesto");

            migrationBuilder.DropTable(
                name: "ProtestoType");

            migrationBuilder.DropTable(
                name: "Corporation");

            migrationBuilder.DropTable(
                name: "EmploymentType");

            migrationBuilder.DropTable(
                name: "ProtestoEmployeeCount");

            migrationBuilder.DropTable(
                name: "Resistance");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "EmployeeCount");

            migrationBuilder.DropTable(
                name: "TradeUnionAuthority");

            migrationBuilder.DropTable(
                name: "TradeUnion");

            migrationBuilder.DropTable(
                name: "CompanyScale");

            migrationBuilder.DropTable(
                name: "CompanyType");

            migrationBuilder.DropTable(
                name: "CompanyWorkLine");

            migrationBuilder.DropTable(
                name: "TradeUnionConfederation");
        }
    }
}
