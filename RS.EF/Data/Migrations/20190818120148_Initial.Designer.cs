﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using RS.EF;
using System;

namespace RS.EF.Data.Migrations
{
    [DbContext(typeof(RSDBContext))]
    [Migration("20190818120148_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("RS.COMMON.Entities.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyScaleId");

                    b.Property<int>("CompanyTypeId");

                    b.Property<int>("CompanyWorkLineId");

                    b.Property<bool>("IsOutsource");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CompanyScaleId");

                    b.HasIndex("CompanyTypeId");

                    b.HasIndex("CompanyWorkLineId");

                    b.ToTable("Company");
                });

            modelBuilder.Entity("RS.COMMON.Entities.CompanyOutsourceCompany", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CompanyId");

                    b.Property<int>("OutsourceCompanyId");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.HasIndex("OutsourceCompanyId");

                    b.ToTable("CompanyOutsourceCompany");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Category");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.City", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("City");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.CompanyScale", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("CompanyScale");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.CompanyType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("CompanyType");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.CompanyWorkLine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("CompanyWorkLine");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.Corporation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Corporation");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.District", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CityId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CityId");

                    b.ToTable("District");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.EmployeeCount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("EmployeeCount");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.EmploymentType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("EmploymentType");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.Gender", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Gender");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.InterventionType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("InterventionType");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.ProtestoEmployeeCount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("ProtestoEmployeeCount");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.ProtestoPlace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("ProtestoPlace");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.ProtestoReason", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("ProtestoReason");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.ProtestoType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("ProtestoType");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.TradeUnion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int?>("TradeUnionConfederationId");

                    b.HasKey("Id");

                    b.HasIndex("TradeUnionConfederationId");

                    b.ToTable("TradeUnion");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.TradeUnionAuthority", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("TradeUnionAuthority");
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.TradeUnionConfederation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("TradeUnionConfederation");
                });

            modelBuilder.Entity("RS.COMMON.Entities.Protesto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AnyLegalIntervention");

                    b.Property<int?>("CustodyCount");

                    b.Property<bool>("DevelopRight");

                    b.Property<int?>("EmployeeCountNumber");

                    b.Property<DateTime?>("EndDate");

                    b.Property<int?>("FiredEmployeeCountByProtesto");

                    b.Property<int>("GenderId");

                    b.Property<bool>("IsAgainstProduction");

                    b.Property<string>("LegalInterventionDesc");

                    b.Property<string>("Note");

                    b.Property<int?>("ProtestoEmployeeCountId");

                    b.Property<int>("ResistanceId");

                    b.Property<DateTime>("StartDate");

                    b.HasKey("Id");

                    b.HasIndex("ProtestoEmployeeCountId");

                    b.HasIndex("ResistanceId");

                    b.ToTable("Protesto");
                });

            modelBuilder.Entity("RS.COMMON.Entities.ProtestoInterventionType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("InterventionTypeId");

                    b.Property<int>("ProtestoId");

                    b.HasKey("Id");

                    b.HasIndex("InterventionTypeId");

                    b.HasIndex("ProtestoId");

                    b.ToTable("ProtestoInterventionType");
                });

            modelBuilder.Entity("RS.COMMON.Entities.ProtestoProtestoPlace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ProtestoId");

                    b.Property<int>("ProtestoPlaceId");

                    b.HasKey("Id");

                    b.HasIndex("ProtestoId");

                    b.HasIndex("ProtestoPlaceId");

                    b.ToTable("ProtestoProtestoPlace");
                });

            modelBuilder.Entity("RS.COMMON.Entities.ProtestoProtestoReason", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ProtestoId");

                    b.Property<int>("ProtestoReasonId");

                    b.HasKey("Id");

                    b.HasIndex("ProtestoId");

                    b.HasIndex("ProtestoReasonId");

                    b.ToTable("ProtestoProtestoReason");
                });

            modelBuilder.Entity("RS.COMMON.Entities.ProtestoProtestoType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ProtestoId");

                    b.Property<int>("ProtestoTypeId");

                    b.HasKey("Id");

                    b.HasIndex("ProtestoId");

                    b.HasIndex("ProtestoTypeId");

                    b.ToTable("ProtestoProtestoType");
                });

            modelBuilder.Entity("RS.COMMON.Entities.Resistance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CategoryId");

                    b.Property<string>("Code");

                    b.Property<int>("CompanyId");

                    b.Property<int?>("EmployeeCountId");

                    b.Property<int?>("EmployeeCountNumber");

                    b.Property<DateTime?>("EndDate");

                    b.Property<bool>("HasTradeUnion");

                    b.Property<DateTime>("StartDate");

                    b.Property<int?>("TradeUnionAuthorityId");

                    b.Property<int?>("TradeUnionId");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("CompanyId");

                    b.HasIndex("EmployeeCountId");

                    b.HasIndex("TradeUnionAuthorityId");

                    b.HasIndex("TradeUnionId");

                    b.ToTable("Resistance");
                });

            modelBuilder.Entity("RS.COMMON.Entities.ResistanceCorporation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CorporationId");

                    b.Property<int>("ResistanceId");

                    b.HasKey("Id");

                    b.HasIndex("CorporationId");

                    b.HasIndex("ResistanceId");

                    b.ToTable("ResistanceCorporation");
                });

            modelBuilder.Entity("RS.COMMON.Entities.ResistanceEmploymentType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("EmploymentTypeId");

                    b.Property<int>("ResistanceId");

                    b.HasKey("Id");

                    b.HasIndex("EmploymentTypeId");

                    b.HasIndex("ResistanceId");

                    b.ToTable("ResistanceEmploymentType");
                });

            modelBuilder.Entity("RS.COMMON.Entities.Company", b =>
                {
                    b.HasOne("RS.COMMON.Entities.LookupEntity.CompanyScale", "CompanyScale")
                        .WithMany()
                        .HasForeignKey("CompanyScaleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.LookupEntity.CompanyType", "CompanyType")
                        .WithMany()
                        .HasForeignKey("CompanyTypeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.LookupEntity.CompanyWorkLine", "CompanyWorkLine")
                        .WithMany()
                        .HasForeignKey("CompanyWorkLineId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.CompanyOutsourceCompany", b =>
                {
                    b.HasOne("RS.COMMON.Entities.Company", "Company")
                        .WithMany("OutsourceCompanies")
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("RS.COMMON.Entities.Company", "OutsourceCompany")
                        .WithMany()
                        .HasForeignKey("OutsourceCompanyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.District", b =>
                {
                    b.HasOne("RS.COMMON.Entities.LookupEntity.City", "City")
                        .WithMany()
                        .HasForeignKey("CityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.LookupEntity.TradeUnion", b =>
                {
                    b.HasOne("RS.COMMON.Entities.LookupEntity.TradeUnionConfederation", "TradeUnionConfederation")
                        .WithMany()
                        .HasForeignKey("TradeUnionConfederationId");
                });

            modelBuilder.Entity("RS.COMMON.Entities.Protesto", b =>
                {
                    b.HasOne("RS.COMMON.Entities.LookupEntity.ProtestoEmployeeCount", "ProtestoEmployeeCount")
                        .WithMany()
                        .HasForeignKey("ProtestoEmployeeCountId");

                    b.HasOne("RS.COMMON.Entities.Resistance", "Resistance")
                        .WithMany("Protestos")
                        .HasForeignKey("ResistanceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.ProtestoInterventionType", b =>
                {
                    b.HasOne("RS.COMMON.Entities.LookupEntity.InterventionType", "InterventionType")
                        .WithMany()
                        .HasForeignKey("InterventionTypeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.Protesto", "Protesto")
                        .WithMany("ProtestoInterventionTypes")
                        .HasForeignKey("ProtestoId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.ProtestoProtestoPlace", b =>
                {
                    b.HasOne("RS.COMMON.Entities.Protesto", "Protesto")
                        .WithMany("ProtestoProtestoPlaces")
                        .HasForeignKey("ProtestoId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.LookupEntity.ProtestoPlace", "ProtestoPlace")
                        .WithMany()
                        .HasForeignKey("ProtestoPlaceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.ProtestoProtestoReason", b =>
                {
                    b.HasOne("RS.COMMON.Entities.Protesto", "Protesto")
                        .WithMany("ProtestoProtestoResaons")
                        .HasForeignKey("ProtestoId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.LookupEntity.ProtestoReason", "ProtestoReason")
                        .WithMany()
                        .HasForeignKey("ProtestoReasonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.ProtestoProtestoType", b =>
                {
                    b.HasOne("RS.COMMON.Entities.Protesto", "Protesto")
                        .WithMany("ProtestoProtestoTypes")
                        .HasForeignKey("ProtestoId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.LookupEntity.ProtestoType", "ProtestoType")
                        .WithMany()
                        .HasForeignKey("ProtestoTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.Resistance", b =>
                {
                    b.HasOne("RS.COMMON.Entities.LookupEntity.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.LookupEntity.EmployeeCount", "EmployeeCount")
                        .WithMany()
                        .HasForeignKey("EmployeeCountId");

                    b.HasOne("RS.COMMON.Entities.LookupEntity.TradeUnionAuthority", "TradeUnionAuthority")
                        .WithMany()
                        .HasForeignKey("TradeUnionAuthorityId");

                    b.HasOne("RS.COMMON.Entities.LookupEntity.TradeUnion", "TradeUnion")
                        .WithMany()
                        .HasForeignKey("TradeUnionId");
                });

            modelBuilder.Entity("RS.COMMON.Entities.ResistanceCorporation", b =>
                {
                    b.HasOne("RS.COMMON.Entities.LookupEntity.Corporation", "Corporation")
                        .WithMany()
                        .HasForeignKey("CorporationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.Resistance", "Resistance")
                        .WithMany("ResistanceCorporations")
                        .HasForeignKey("ResistanceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RS.COMMON.Entities.ResistanceEmploymentType", b =>
                {
                    b.HasOne("RS.COMMON.Entities.LookupEntity.EmploymentType", "EmploymentType")
                        .WithMany()
                        .HasForeignKey("EmploymentTypeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("RS.COMMON.Entities.Resistance", "Resistance")
                        .WithMany("ResistanceEmploymentTypes")
                        .HasForeignKey("ResistanceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
