using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RS.EF.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyScale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyScale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyWorkLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyWorkLine", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorporationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeCount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gender",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gender", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterventionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Header = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoEmployeeCount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoEmployeeCount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoPlace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoPlace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgainstProduction = table.Column<bool>(type: "boolean", nullable: false),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    Simple = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResistanceReason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResistanceReason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeUnionAuthority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeUnionAuthority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeUnionConfederation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeUnionConfederation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CompanyWorkLineId = table.Column<int>(type: "integer", nullable: true),
                    CompanyScaleId = table.Column<int>(type: "integer", nullable: true),
                    CompanyTypeId = table.Column<int>(type: "integer", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Company_CompanyScale_CompanyScaleId",
                        column: x => x.CompanyScaleId,
                        principalTable: "CompanyScale",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Company_CompanyType_CompanyTypeId",
                        column: x => x.CompanyTypeId,
                        principalTable: "CompanyType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Company_CompanyWorkLine_CompanyWorkLineId",
                        column: x => x.CompanyWorkLineId,
                        principalTable: "CompanyWorkLine",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Corporation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    CorporationTypeId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corporation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Corporation_CorporationType_CorporationTypeId",
                        column: x => x.CorporationTypeId,
                        principalTable: "CorporationType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TradeUnion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TradeUnionConfederationId = table.Column<int>(type: "integer", nullable: true),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeUnion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeUnion_TradeUnionConfederation_TradeUnionConfederationId",
                        column: x => x.TradeUnionConfederationId,
                        principalTable: "TradeUnionConfederation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyOutsourceCompany",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    OutsourceCompanyId = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    MainCompanyId = table.Column<int>(type: "integer", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: true),
                    EmployeeCountNumber = table.Column<int>(type: "integer", nullable: true),
                    EmployeeCountId = table.Column<int>(type: "integer", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HasTradeUnion = table.Column<bool>(type: "boolean", nullable: false),
                    DevelopRight = table.Column<bool>(type: "boolean", nullable: false),
                    TradeUnionId = table.Column<int>(type: "integer", nullable: true),
                    TradeUnionAuthorityId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AnyLegalIntervention = table.Column<bool>(type: "boolean", nullable: true),
                    LegalInterventionDesc = table.Column<string>(type: "text", nullable: true),
                    FiredEmployeeCountByProtesto = table.Column<int>(type: "integer", nullable: true),
                    ResistanceResult = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Creator = table.Column<string>(type: "text", nullable: true),
                    Updater = table.Column<string>(type: "text", nullable: true)
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
                        name: "FK_Resistance_Company_MainCompanyId",
                        column: x => x.MainCompanyId,
                        principalTable: "Company",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Resistance_Corporation_TradeUnionId",
                        column: x => x.TradeUnionId,
                        principalTable: "Corporation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Resistance_EmployeeCount_EmployeeCountId",
                        column: x => x.EmployeeCountId,
                        principalTable: "EmployeeCount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Resistance_TradeUnionAuthority_TradeUnionAuthorityId",
                        column: x => x.TradeUnionAuthorityId,
                        principalTable: "TradeUnionAuthority",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Protesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResistanceId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmployeeCountNumber = table.Column<int>(type: "integer", nullable: true),
                    ProtestoEmployeeCountId = table.Column<int>(type: "integer", nullable: true),
                    GenderId = table.Column<int>(type: "integer", nullable: false),
                    CustodyCount = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    SimpleProtestoDescription = table.Column<string>(type: "text", nullable: true),
                    StrikeDuration = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Creator = table.Column<string>(type: "text", nullable: true),
                    Updater = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Protesto_Gender_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Gender",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Protesto_ProtestoEmployeeCount_ProtestoEmployeeCountId",
                        column: x => x.ProtestoEmployeeCountId,
                        principalTable: "ProtestoEmployeeCount",
                        principalColumn: "Id");
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResistanceId = table.Column<int>(type: "integer", nullable: false),
                    CorporationId = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResistanceId = table.Column<int>(type: "integer", nullable: false),
                    EmploymentTypeId = table.Column<int>(type: "integer", nullable: false)
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
                name: "ResistanceNews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResistanceId = table.Column<int>(type: "integer", nullable: false),
                    NewsId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ResistanceResistanceReason",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResistanceId = table.Column<int>(type: "integer", nullable: false),
                    ResistanceReasonId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResistanceResistanceReason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResistanceResistanceReason_Resistance_ResistanceId",
                        column: x => x.ResistanceId,
                        principalTable: "Resistance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResistanceResistanceReason_ResistanceReason_ResistanceReaso~",
                        column: x => x.ResistanceReasonId,
                        principalTable: "ResistanceReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoCity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProtestoId = table.Column<int>(type: "integer", nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProtestoId = table.Column<int>(type: "integer", nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ProtestoInterventionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProtestoId = table.Column<int>(type: "integer", nullable: false),
                    InterventionTypeId = table.Column<int>(type: "integer", nullable: false)
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
                name: "ProtestoLocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProtestoId = table.Column<int>(type: "integer", nullable: false),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: true),
                    Place = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtestoLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtestoLocation_City_CityId",
                        column: x => x.CityId,
                        principalTable: "City",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtestoLocation_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProtestoLocation_Protesto_ProtestoId",
                        column: x => x.ProtestoId,
                        principalTable: "Protesto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtestoProtestoPlace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProtestoId = table.Column<int>(type: "integer", nullable: false),
                    ProtestoPlaceId = table.Column<int>(type: "integer", nullable: false)
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
                name: "ProtestoProtestoType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProtestoId = table.Column<int>(type: "integer", nullable: false),
                    ProtestoTypeId = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_Corporation_CorporationTypeId",
                table: "Corporation",
                column: "CorporationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_District_CityId",
                table: "District",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Protesto_GenderId",
                table: "Protesto",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Protesto_ProtestoEmployeeCountId",
                table: "Protesto",
                column: "ProtestoEmployeeCountId");

            migrationBuilder.CreateIndex(
                name: "IX_Protesto_ResistanceId",
                table: "Protesto",
                column: "ResistanceId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoInterventionType_InterventionTypeId",
                table: "ProtestoInterventionType",
                column: "InterventionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoInterventionType_ProtestoId",
                table: "ProtestoInterventionType",
                column: "ProtestoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoLocation_CityId",
                table: "ProtestoLocation",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoLocation_DistrictId",
                table: "ProtestoLocation",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtestoLocation_ProtestoId",
                table: "ProtestoLocation",
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
                name: "IX_Resistance_MainCompanyId",
                table: "Resistance",
                column: "MainCompanyId");

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
                name: "IX_ResistanceNews_NewsId",
                table: "ResistanceNews",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceNews_ResistanceId",
                table: "ResistanceNews",
                column: "ResistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceResistanceReason_ResistanceId",
                table: "ResistanceResistanceReason",
                column: "ResistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResistanceResistanceReason_ResistanceReasonId",
                table: "ResistanceResistanceReason",
                column: "ResistanceReasonId");

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
                name: "ProtestoCity");

            migrationBuilder.DropTable(
                name: "ProtestoDistrict");

            migrationBuilder.DropTable(
                name: "ProtestoInterventionType");

            migrationBuilder.DropTable(
                name: "ProtestoLocation");

            migrationBuilder.DropTable(
                name: "ProtestoProtestoPlace");

            migrationBuilder.DropTable(
                name: "ProtestoProtestoType");

            migrationBuilder.DropTable(
                name: "ResistanceCorporation");

            migrationBuilder.DropTable(
                name: "ResistanceEmploymentType");

            migrationBuilder.DropTable(
                name: "ResistanceNews");

            migrationBuilder.DropTable(
                name: "ResistanceResistanceReason");

            migrationBuilder.DropTable(
                name: "TradeUnion");

            migrationBuilder.DropTable(
                name: "InterventionType");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "ProtestoPlace");

            migrationBuilder.DropTable(
                name: "Protesto");

            migrationBuilder.DropTable(
                name: "ProtestoType");

            migrationBuilder.DropTable(
                name: "EmploymentType");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "ResistanceReason");

            migrationBuilder.DropTable(
                name: "TradeUnionConfederation");

            migrationBuilder.DropTable(
                name: "City");

            migrationBuilder.DropTable(
                name: "Gender");

            migrationBuilder.DropTable(
                name: "ProtestoEmployeeCount");

            migrationBuilder.DropTable(
                name: "Resistance");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "Corporation");

            migrationBuilder.DropTable(
                name: "EmployeeCount");

            migrationBuilder.DropTable(
                name: "TradeUnionAuthority");

            migrationBuilder.DropTable(
                name: "CompanyScale");

            migrationBuilder.DropTable(
                name: "CompanyType");

            migrationBuilder.DropTable(
                name: "CompanyWorkLine");

            migrationBuilder.DropTable(
                name: "CorporationType");
        }
    }
}
