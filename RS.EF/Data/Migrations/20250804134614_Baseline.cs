using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// The production database as it was in October 2026, before the redesign (F12). It replaces the sixteen migrations
    /// from 2019–2025: production had been changed by hand since (tables, columns and types no migration created), so they
    /// could no longer build a working database. Scripted from production's schema with sqlpackage (structure only).
    /// On a database that already has the tables (production, the test copy) it does nothing; on an empty one it builds them.
    /// SchemaFixes, after the redesign's migrations, then brings the schema to what the model defines.
    /// </summary>
    [DbContext(typeof(RSDBContext))]
    [Migration("20250804134614_Baseline")]
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
            IF OBJECT_ID(N'dbo.Resistance') IS NULL
            BEGIN
            EXEC(N'CREATE TABLE [dbo].[Category] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[City] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_City] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[Company] (
                    [Id]                INT            IDENTITY (1, 1) NOT NULL,
                    [CompanyScaleId]    INT            NULL,
                    [CompanyTypeId]     INT            NULL,
                    [CompanyWorkLineId] INT            NULL,
                    [Name]              NVARCHAR (MAX) NULL,
                    [Deleted]           BIT            NOT NULL,
                    CONSTRAINT [PK_Company] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Company_CompanyWorkLineId]
                    ON [dbo].[Company]([CompanyWorkLineId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Company_CompanyTypeId]
                    ON [dbo].[Company]([CompanyTypeId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Company_CompanyScaleId]
                    ON [dbo].[Company]([CompanyScaleId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[CompanyOutsourceCompany] (
                    [Id]                 INT IDENTITY (1, 1) NOT NULL,
                    [CompanyId]          INT NOT NULL,
                    [OutsourceCompanyId] INT NOT NULL,
                    CONSTRAINT [PK_CompanyOutsourceCompany] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_CompanyOutsourceCompany_OutsourceCompanyId]
                    ON [dbo].[CompanyOutsourceCompany]([OutsourceCompanyId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_CompanyOutsourceCompany_CompanyId]
                    ON [dbo].[CompanyOutsourceCompany]([CompanyId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[CompanyScale] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_CompanyScale] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[CompanyType] (
                    [Id]          INT            IDENTITY (1, 1) NOT NULL,
                    [Description] NVARCHAR (MAX) NULL,
                    [Name]        NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_CompanyType] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[CompanyWorkLine] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_CompanyWorkLine] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[Corporation] (
                    [Id]                INT            IDENTITY (1, 1) NOT NULL,
                    [Name]              NVARCHAR (MAX) NULL,
                    [Approved]          BIT            NOT NULL,
                    [CorporationTypeId] INT            NULL,
                    CONSTRAINT [PK_Corporation] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[CorporationType] (
                    [Id]   INT           IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (25) NULL,
                    CONSTRAINT [PK_CorporationType] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[District] (
                    [Id]     INT            IDENTITY (1, 1) NOT NULL,
                    [CityId] INT            NOT NULL,
                    [Name]   NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_District] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_District_CityId]
                    ON [dbo].[District]([CityId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[EmployeeCount] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_EmployeeCount] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[EmploymentType] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_EmploymentType] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[Gender] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_Gender] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[InterventionType] (
                    [Id]          INT            IDENTITY (1, 1) NOT NULL,
                    [Description] NVARCHAR (MAX) NULL,
                    [Name]        NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_InterventionType] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[News] (
                    [Id]      INT            IDENTITY (1, 1) NOT NULL,
                    [Header]  NVARCHAR (MAX) NULL,
                    [Content] NVARCHAR (MAX) NULL,
                    [Date]    DATETIME2 (7)  NOT NULL,
                    [Link]    NVARCHAR (MAX) NULL,
                    [Status]  INT            NOT NULL,
                    [Source]  INT            NOT NULL,
                    CONSTRAINT [PK_News] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[Protesto] (
                    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
                    [CustodyCount]              INT            NULL,
                    [EmployeeCountNumber]       INT            NULL,
                    [EndDate]                   DATETIME2 (7)  NULL,
                    [GenderId]                  INT            NOT NULL,
                    [Note]                      NVARCHAR (MAX) NULL,
                    [ProtestoEmployeeCountId]   INT            NULL,
                    [ResistanceId]              INT            NOT NULL,
                    [StartDate]                 DATETIME2 (7)  NOT NULL,
                    [Creator]                   NVARCHAR (50)  NULL,
                    [Updater]                   NVARCHAR (50)  NULL,
                    [CreateDate]                DATE           NULL,
                    [UpdateDate]                DATE           NULL,
                    [Deleted]                   BIT            NOT NULL,
                    [SimpleProtestoDescription] NVARCHAR (MAX) NULL,
                    [StrikeDuration]            INT            NOT NULL,
                    CONSTRAINT [PK_Protesto] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Protesto_ResistanceId]
                    ON [dbo].[Protesto]([ResistanceId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Protesto_ProtestoEmployeeCountId]
                    ON [dbo].[Protesto]([ProtestoEmployeeCountId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[ProtestoCity] (
                    [Id]         INT IDENTITY (1, 1) NOT NULL,
                    [ProtestoId] INT NOT NULL,
                    [CityId]     INT NOT NULL,
                    CONSTRAINT [PK_ProtestoCity] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[ProtestoDistrict] (
                    [Id]         INT IDENTITY (1, 1) NOT NULL,
                    [ProtestoId] INT NOT NULL,
                    [DistrictId] INT NOT NULL,
                    CONSTRAINT [PK_ProtestoDistrict] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[ProtestoEmployeeCount] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_ProtestoEmployeeCount] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[ProtestoInterventionType] (
                    [Id]                 INT IDENTITY (1, 1) NOT NULL,
                    [InterventionTypeId] INT NOT NULL,
                    [ProtestoId]         INT NOT NULL,
                    CONSTRAINT [PK_ProtestoInterventionType] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ProtestoInterventionType_ProtestoId]
                    ON [dbo].[ProtestoInterventionType]([ProtestoId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ProtestoInterventionType_InterventionTypeId]
                    ON [dbo].[ProtestoInterventionType]([InterventionTypeId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[ProtestoLocation] (
                    [Id]         INT         IDENTITY (1, 1) NOT NULL,
                    [ProtestoId] INT         NOT NULL,
                    [CityId]     INT         NOT NULL,
                    [DistrictId] INT         NULL,
                    [Place]      NCHAR (500) NULL,
                    CONSTRAINT [PK_ProtestoLocation] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[ProtestoPlace] (
                    [Id]       INT            IDENTITY (1, 1) NOT NULL,
                    [Name]     NVARCHAR (MAX) NULL,
                    [Approved] BIT            NOT NULL,
                    CONSTRAINT [PK_ProtestoPlace] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[ProtestoProtestoPlace] (
                    [Id]              INT IDENTITY (1, 1) NOT NULL,
                    [ProtestoId]      INT NOT NULL,
                    [ProtestoPlaceId] INT NOT NULL,
                    CONSTRAINT [PK_ProtestoProtestoPlace] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ProtestoProtestoPlace_ProtestoId]
                    ON [dbo].[ProtestoProtestoPlace]([ProtestoId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ProtestoProtestoPlace_ProtestoPlaceId]
                    ON [dbo].[ProtestoProtestoPlace]([ProtestoPlaceId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[ProtestoProtestoType] (
                    [Id]             INT IDENTITY (1, 1) NOT NULL,
                    [ProtestoId]     INT NOT NULL,
                    [ProtestoTypeId] INT NOT NULL,
                    CONSTRAINT [PK_ProtestoProtestoType] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ProtestoProtestoType_ProtestoId]
                    ON [dbo].[ProtestoProtestoType]([ProtestoId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ProtestoProtestoType_ProtestoTypeId]
                    ON [dbo].[ProtestoProtestoType]([ProtestoTypeId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[ProtestoType] (
                    [Id]                INT            IDENTITY (1, 1) NOT NULL,
                    [Name]              NVARCHAR (MAX) NULL,
                    [Approved]          BIT            NOT NULL,
                    [AgainstProduction] BIT            NOT NULL,
                    [Simple]            BIT            NOT NULL,
                    CONSTRAINT [PK_ProtestoType] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[Resistance] (
                    [Id]                           INT            IDENTITY (1, 1) NOT NULL,
                    [CategoryId]                   INT            NOT NULL,
                    [Code]                         NVARCHAR (MAX) NULL,
                    [CompanyId]                    INT            NULL,
                    [EmployeeCountId]              INT            NULL,
                    [EmployeeCountNumber]          INT            NULL,
                    [EndDate]                      DATETIME2 (7)  NULL,
                    [HasTradeUnion]                BIT            NOT NULL,
                    [StartDate]                    DATETIME2 (7)  NOT NULL,
                    [TradeUnionAuthorityId]        INT            NULL,
                    [TradeUnionId]                 INT            NULL,
                    [DevelopRight]                 BIT            NOT NULL,
                    [Note]                         NVARCHAR (MAX) NULL,
                    [description]                  NVARCHAR (MAX) NULL,
                    [AnyLegalIntervention]         BIT            NULL,
                    [LegalInterventionDesc]        NVARCHAR (MAX) NULL,
                    [FiredEmployeeCountByProtesto] INT            NULL,
                    [Creator]                      NVARCHAR (50)  NULL,
                    [Updater]                      NVARCHAR (50)  NULL,
                    [CreateDate]                   DATE           NULL,
                    [UpdateDate]                   DATE           NULL,
                    [Deleted]                      BIT            NOT NULL,
                    [MainCompanyId]                INT            NULL,
                    [ResistanceResult]             INT            NOT NULL,
                    [Name]                         NVARCHAR (MAX) NULL,
                    [CorporationId]                INT            NULL,
                    [ResistanceTargetType]         INT            NOT NULL,
                    CONSTRAINT [PK_Resistance] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Resistance_CorporationId]
                    ON [dbo].[Resistance]([CorporationId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Resistance_CompanyId]
                    ON [dbo].[Resistance]([CompanyId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Resistance_TradeUnionId]
                    ON [dbo].[Resistance]([TradeUnionId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Resistance_CategoryId]
                    ON [dbo].[Resistance]([CategoryId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Resistance_TradeUnionAuthorityId]
                    ON [dbo].[Resistance]([TradeUnionAuthorityId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_Resistance_EmployeeCountId]
                    ON [dbo].[Resistance]([EmployeeCountId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[ResistanceCorporation] (
                    [Id]            INT IDENTITY (1, 1) NOT NULL,
                    [CorporationId] INT NOT NULL,
                    [ResistanceId]  INT NOT NULL,
                    CONSTRAINT [PK_ResistanceCorporation] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ResistanceCorporation_ResistanceId]
                    ON [dbo].[ResistanceCorporation]([ResistanceId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ResistanceCorporation_CorporationId]
                    ON [dbo].[ResistanceCorporation]([CorporationId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[ResistanceEmploymentType] (
                    [Id]               INT IDENTITY (1, 1) NOT NULL,
                    [EmploymentTypeId] INT NOT NULL,
                    [ResistanceId]     INT NOT NULL,
                    CONSTRAINT [PK_ResistanceEmploymentType] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ResistanceEmploymentType_EmploymentTypeId]
                    ON [dbo].[ResistanceEmploymentType]([EmploymentTypeId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ResistanceEmploymentType_ResistanceId]
                    ON [dbo].[ResistanceEmploymentType]([ResistanceId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[ResistanceNews] (
                    [Id]           INT IDENTITY (1, 1) NOT NULL,
                    [ResistanceId] INT NOT NULL,
                    [NewsId]       INT NOT NULL,
                    CONSTRAINT [PK_ResistanceNews] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ResistanceNews_ResistanceId]
                    ON [dbo].[ResistanceNews]([ResistanceId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ResistanceNews_NewsId]
                    ON [dbo].[ResistanceNews]([NewsId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[ResistanceReason] (
                    [Id]       INT            IDENTITY (1, 1) NOT NULL,
                    [Name]     NVARCHAR (MAX) NULL,
                    [Approved] BIT            NOT NULL,
                    CONSTRAINT [PK_ResistanceReason] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[ResistanceResistanceReason] (
                    [Id]                 INT IDENTITY (1, 1) NOT NULL,
                    [ResistanceId]       INT NOT NULL,
                    [ResistanceReasonId] INT NOT NULL,
                    CONSTRAINT [PK_ResistanceResistanceReason] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ResistanceResistanceReason_ResistanceReasonId]
                    ON [dbo].[ResistanceResistanceReason]([ResistanceReasonId] ASC);');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_ResistanceResistanceReason_ResistanceId]
                    ON [dbo].[ResistanceResistanceReason]([ResistanceId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[TradeUnion] (
                    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
                    [Name]                      NVARCHAR (MAX) NULL,
                    [TradeUnionConfederationId] INT            NULL,
                    [Approved]                  BIT            NOT NULL,
                    CONSTRAINT [PK_TradeUnion] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE NONCLUSTERED INDEX [IX_TradeUnion_TradeUnionConfederationId]
                    ON [dbo].[TradeUnion]([TradeUnionConfederationId] ASC);');
            EXEC(N'CREATE TABLE [dbo].[TradeUnionAuthority] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_TradeUnionAuthority] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'CREATE TABLE [dbo].[TradeUnionConfederation] (
                    [Id]   INT            IDENTITY (1, 1) NOT NULL,
                    [Name] NVARCHAR (MAX) NULL,
                    CONSTRAINT [PK_TradeUnionConfederation] PRIMARY KEY CLUSTERED ([Id] ASC)
                );');
            EXEC(N'ALTER TABLE [dbo].[Company]
                    ADD DEFAULT ((0)) FOR [Deleted];');
            EXEC(N'ALTER TABLE [dbo].[Corporation]
                    ADD DEFAULT ((0)) FOR [Approved];');
            EXEC(N'ALTER TABLE [dbo].[News]
                    ADD CONSTRAINT [DF__News__Status__14270015] DEFAULT ((0)) FOR [Status];');
            EXEC(N'ALTER TABLE [dbo].[Protesto]
                    ADD DEFAULT ((0)) FOR [Deleted];');
            EXEC(N'ALTER TABLE [dbo].[Protesto]
                    ADD DEFAULT ((0)) FOR [StrikeDuration];');
            EXEC(N'ALTER TABLE [dbo].[ProtestoType]
                    ADD CONSTRAINT [DF__ProtestoT__Again__41B8C09B] DEFAULT ((0)) FOR [AgainstProduction];');
            EXEC(N'ALTER TABLE [dbo].[Resistance]
                    ADD CONSTRAINT [DF__Resistanc__Delet__0697FACD] DEFAULT ((0)) FOR [Deleted];');
            EXEC(N'ALTER TABLE [dbo].[Resistance]
                    ADD DEFAULT ((0)) FOR [ResistanceTargetType];');
            EXEC(N'ALTER TABLE [dbo].[Company] WITH NOCHECK
                    ADD CONSTRAINT [FK_Company_CompanyScale_CompanyScaleId] FOREIGN KEY ([CompanyScaleId]) REFERENCES [dbo].[CompanyScale] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[Company] WITH NOCHECK
                    ADD CONSTRAINT [FK_Company_CompanyType_CompanyTypeId] FOREIGN KEY ([CompanyTypeId]) REFERENCES [dbo].[CompanyType] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[Company] WITH NOCHECK
                    ADD CONSTRAINT [FK_Company_CompanyWorkLine_CompanyWorkLineId] FOREIGN KEY ([CompanyWorkLineId]) REFERENCES [dbo].[CompanyWorkLine] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[CompanyOutsourceCompany] WITH NOCHECK
                    ADD CONSTRAINT [FK_CompanyOutsourceCompany_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Company] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[CompanyOutsourceCompany] WITH NOCHECK
                    ADD CONSTRAINT [FK_CompanyOutsourceCompany_Company_OutsourceCompanyId] FOREIGN KEY ([OutsourceCompanyId]) REFERENCES [dbo].[Company] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[Corporation] WITH NOCHECK
                    ADD CONSTRAINT [FK_Corporation_CompanyType] FOREIGN KEY ([CorporationTypeId]) REFERENCES [dbo].[CompanyType] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[District] WITH NOCHECK
                    ADD CONSTRAINT [FK_District_City_CityId] FOREIGN KEY ([CityId]) REFERENCES [dbo].[City] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[Protesto] WITH NOCHECK
                    ADD CONSTRAINT [FK_Protesto_ProtestoEmployeeCount_ProtestoEmployeeCountId] FOREIGN KEY ([ProtestoEmployeeCountId]) REFERENCES [dbo].[ProtestoEmployeeCount] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[Protesto] WITH NOCHECK
                    ADD CONSTRAINT [FK_Protesto_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [dbo].[Resistance] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoCity] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoCity_City_CityId] FOREIGN KEY ([CityId]) REFERENCES [dbo].[City] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoCity] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoCity_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [dbo].[Protesto] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoDistrict] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoDistrict_District_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [dbo].[District] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoDistrict] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoDistrict_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [dbo].[Protesto] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoInterventionType] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoInterventionType_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [dbo].[Protesto] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoInterventionType] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoInterventionType_InterventionType_InterventionTypeId] FOREIGN KEY ([InterventionTypeId]) REFERENCES [dbo].[InterventionType] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoLocation] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoLocation_ProtestoLocation] FOREIGN KEY ([ProtestoId]) REFERENCES [dbo].[Protesto] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[ProtestoLocation] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoLocation_District] FOREIGN KEY ([DistrictId]) REFERENCES [dbo].[District] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[ProtestoLocation] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoLocation_City] FOREIGN KEY ([CityId]) REFERENCES [dbo].[City] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[ProtestoProtestoPlace] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoProtestoPlace_ProtestoPlace_ProtestoPlaceId] FOREIGN KEY ([ProtestoPlaceId]) REFERENCES [dbo].[ProtestoPlace] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoProtestoPlace] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoProtestoPlace_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [dbo].[Protesto] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoProtestoType] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoProtestoType_ProtestoType_ProtestoTypeId] FOREIGN KEY ([ProtestoTypeId]) REFERENCES [dbo].[ProtestoType] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ProtestoProtestoType] WITH NOCHECK
                    ADD CONSTRAINT [FK_ProtestoProtestoType_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [dbo].[Protesto] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[Resistance] WITH NOCHECK
                    ADD CONSTRAINT [FK_Resistance_Corporation_CorporationId] FOREIGN KEY ([CorporationId]) REFERENCES [dbo].[Corporation] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[Resistance] WITH NOCHECK
                    ADD CONSTRAINT [FK_Resistance_Corporation_TradeUnionId] FOREIGN KEY ([TradeUnionId]) REFERENCES [dbo].[Corporation] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[Resistance] WITH NOCHECK
                    ADD CONSTRAINT [FK_Resistance_EmployeeCount_EmployeeCountId] FOREIGN KEY ([EmployeeCountId]) REFERENCES [dbo].[EmployeeCount] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[Resistance] WITH NOCHECK
                    ADD CONSTRAINT [FK_Resistance_Category_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[Resistance] WITH NOCHECK
                    ADD CONSTRAINT [FK_Resistance_TradeUnionAuthority_TradeUnionAuthorityId] FOREIGN KEY ([TradeUnionAuthorityId]) REFERENCES [dbo].[TradeUnionAuthority] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[Resistance] WITH NOCHECK
                    ADD CONSTRAINT [FK_Resistance_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[Company] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[ResistanceCorporation] WITH NOCHECK
                    ADD CONSTRAINT [FK_ResistanceCorporation_Corporation_CorporationId] FOREIGN KEY ([CorporationId]) REFERENCES [dbo].[Corporation] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ResistanceCorporation] WITH NOCHECK
                    ADD CONSTRAINT [FK_ResistanceCorporation_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [dbo].[Resistance] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ResistanceEmploymentType] WITH NOCHECK
                    ADD CONSTRAINT [FK_ResistanceEmploymentType_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [dbo].[Resistance] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ResistanceEmploymentType] WITH NOCHECK
                    ADD CONSTRAINT [FK_ResistanceEmploymentType_EmploymentType_EmploymentTypeId] FOREIGN KEY ([EmploymentTypeId]) REFERENCES [dbo].[EmploymentType] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ResistanceNews] WITH NOCHECK
                    ADD CONSTRAINT [FK_ResistanceNews_News_NewsId] FOREIGN KEY ([NewsId]) REFERENCES [dbo].[News] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ResistanceNews] WITH NOCHECK
                    ADD CONSTRAINT [FK_ResistanceNews_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [dbo].[Resistance] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ResistanceResistanceReason] WITH NOCHECK
                    ADD CONSTRAINT [FK_ResistanceResistanceReason_ResistanceReason_ResistanceReasonId] FOREIGN KEY ([ResistanceReasonId]) REFERENCES [dbo].[ResistanceReason] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[ResistanceResistanceReason] WITH NOCHECK
                    ADD CONSTRAINT [FK_ResistanceResistanceReason_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [dbo].[Resistance] ([Id]) ON DELETE CASCADE;');
            EXEC(N'ALTER TABLE [dbo].[TradeUnion] WITH NOCHECK
                    ADD CONSTRAINT [FK_TradeUnion_TradeUnionConfederation_TradeUnionConfederationId] FOREIGN KEY ([TradeUnionConfederationId]) REFERENCES [dbo].[TradeUnionConfederation] ([Id]);');
            EXEC(N'ALTER TABLE [dbo].[Company] WITH CHECK CHECK CONSTRAINT [FK_Company_CompanyScale_CompanyScaleId];
                ALTER TABLE [dbo].[Company] WITH CHECK CHECK CONSTRAINT [FK_Company_CompanyType_CompanyTypeId];
                ALTER TABLE [dbo].[Company] WITH CHECK CHECK CONSTRAINT [FK_Company_CompanyWorkLine_CompanyWorkLineId];
                ALTER TABLE [dbo].[CompanyOutsourceCompany] WITH CHECK CHECK CONSTRAINT [FK_CompanyOutsourceCompany_Company_CompanyId];
                ALTER TABLE [dbo].[CompanyOutsourceCompany] WITH CHECK CHECK CONSTRAINT [FK_CompanyOutsourceCompany_Company_OutsourceCompanyId];
                ALTER TABLE [dbo].[Corporation] WITH CHECK CHECK CONSTRAINT [FK_Corporation_CompanyType];
                ALTER TABLE [dbo].[District] WITH CHECK CHECK CONSTRAINT [FK_District_City_CityId];
                ALTER TABLE [dbo].[Protesto] WITH CHECK CHECK CONSTRAINT [FK_Protesto_ProtestoEmployeeCount_ProtestoEmployeeCountId];
                ALTER TABLE [dbo].[Protesto] WITH CHECK CHECK CONSTRAINT [FK_Protesto_Resistance_ResistanceId];
                ALTER TABLE [dbo].[ProtestoCity] WITH CHECK CHECK CONSTRAINT [FK_ProtestoCity_City_CityId];
                ALTER TABLE [dbo].[ProtestoCity] WITH CHECK CHECK CONSTRAINT [FK_ProtestoCity_Protesto_ProtestoId];
                ALTER TABLE [dbo].[ProtestoDistrict] WITH CHECK CHECK CONSTRAINT [FK_ProtestoDistrict_District_DistrictId];
                ALTER TABLE [dbo].[ProtestoDistrict] WITH CHECK CHECK CONSTRAINT [FK_ProtestoDistrict_Protesto_ProtestoId];
                ALTER TABLE [dbo].[ProtestoInterventionType] WITH CHECK CHECK CONSTRAINT [FK_ProtestoInterventionType_Protesto_ProtestoId];
                ALTER TABLE [dbo].[ProtestoInterventionType] WITH CHECK CHECK CONSTRAINT [FK_ProtestoInterventionType_InterventionType_InterventionTypeId];
                ALTER TABLE [dbo].[ProtestoLocation] WITH CHECK CHECK CONSTRAINT [FK_ProtestoLocation_ProtestoLocation];
                ALTER TABLE [dbo].[ProtestoLocation] WITH CHECK CHECK CONSTRAINT [FK_ProtestoLocation_District];
                ALTER TABLE [dbo].[ProtestoLocation] WITH CHECK CHECK CONSTRAINT [FK_ProtestoLocation_City];
                ALTER TABLE [dbo].[ProtestoProtestoPlace] WITH CHECK CHECK CONSTRAINT [FK_ProtestoProtestoPlace_ProtestoPlace_ProtestoPlaceId];
                ALTER TABLE [dbo].[ProtestoProtestoPlace] WITH CHECK CHECK CONSTRAINT [FK_ProtestoProtestoPlace_Protesto_ProtestoId];
                ALTER TABLE [dbo].[ProtestoProtestoType] WITH CHECK CHECK CONSTRAINT [FK_ProtestoProtestoType_ProtestoType_ProtestoTypeId];
                ALTER TABLE [dbo].[ProtestoProtestoType] WITH CHECK CHECK CONSTRAINT [FK_ProtestoProtestoType_Protesto_ProtestoId];
                ALTER TABLE [dbo].[Resistance] WITH CHECK CHECK CONSTRAINT [FK_Resistance_Corporation_CorporationId];
                ALTER TABLE [dbo].[Resistance] WITH CHECK CHECK CONSTRAINT [FK_Resistance_Corporation_TradeUnionId];
                ALTER TABLE [dbo].[Resistance] WITH CHECK CHECK CONSTRAINT [FK_Resistance_EmployeeCount_EmployeeCountId];
                ALTER TABLE [dbo].[Resistance] WITH CHECK CHECK CONSTRAINT [FK_Resistance_Category_CategoryId];
                ALTER TABLE [dbo].[Resistance] WITH CHECK CHECK CONSTRAINT [FK_Resistance_TradeUnionAuthority_TradeUnionAuthorityId];
                ALTER TABLE [dbo].[Resistance] WITH CHECK CHECK CONSTRAINT [FK_Resistance_Company_CompanyId];
                ALTER TABLE [dbo].[ResistanceCorporation] WITH CHECK CHECK CONSTRAINT [FK_ResistanceCorporation_Corporation_CorporationId];
                ALTER TABLE [dbo].[ResistanceCorporation] WITH CHECK CHECK CONSTRAINT [FK_ResistanceCorporation_Resistance_ResistanceId];
                ALTER TABLE [dbo].[ResistanceEmploymentType] WITH CHECK CHECK CONSTRAINT [FK_ResistanceEmploymentType_Resistance_ResistanceId];
                ALTER TABLE [dbo].[ResistanceEmploymentType] WITH CHECK CHECK CONSTRAINT [FK_ResistanceEmploymentType_EmploymentType_EmploymentTypeId];
                ALTER TABLE [dbo].[ResistanceNews] WITH CHECK CHECK CONSTRAINT [FK_ResistanceNews_News_NewsId];
                ALTER TABLE [dbo].[ResistanceNews] WITH CHECK CHECK CONSTRAINT [FK_ResistanceNews_Resistance_ResistanceId];
                ALTER TABLE [dbo].[ResistanceResistanceReason] WITH CHECK CHECK CONSTRAINT [FK_ResistanceResistanceReason_ResistanceReason_ResistanceReasonId];
                ALTER TABLE [dbo].[ResistanceResistanceReason] WITH CHECK CHECK CONSTRAINT [FK_ResistanceResistanceReason_Resistance_ResistanceId];
                ALTER TABLE [dbo].[TradeUnion] WITH CHECK CHECK CONSTRAINT [FK_TradeUnion_TradeUnionConfederation_TradeUnionConfederationId];');
            END
            """);

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            throw new NotSupportedException("The baseline is where the history starts; it is not undone.");
    }
}
