using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchemaFixes : Migration
    {
        // Production was changed by hand over the years, so instead of altering what EF believes is there, this removes every
        // foreign key, default, secondary index and automatic statistic of the app's tables and builds them again exactly as
        // the model defines them. The result is the same on production, its test copy and a database built from Baseline.
        // The steps run in one transaction (EF wraps each migration in one), so a failure leaves the database as it was.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Remove foreign keys, default values, secondary indexes and automatic statistics (they block type changes).
            migrationBuilder.Sql("""
            DECLARE @sql nvarchar(max) = N'';
            SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id))
                + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
            FROM sys.foreign_keys WHERE OBJECT_NAME(parent_object_id) <> '__EFMigrationsHistory';
            SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id))
                + N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
            FROM sys.default_constraints WHERE OBJECT_NAME(parent_object_id) <> '__EFMigrationsHistory';
            SELECT @sql += N'DROP INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N';'
            FROM sys.indexes i JOIN sys.tables t ON t.object_id = i.object_id
            WHERE i.is_primary_key = 0 AND i.is_unique_constraint = 0 AND i.type > 0 AND t.is_ms_shipped = 0 AND t.name <> '__EFMigrationsHistory';
            SELECT @sql += N'DROP STATISTICS ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N'.' + QUOTENAME(s.name) + N';'
            FROM sys.stats s JOIN sys.tables t ON t.object_id = s.object_id
            WHERE s.auto_created = 1 AND t.is_ms_shipped = 0 AND t.name <> '__EFMigrationsHistory';
            EXEC sp_executesql @sql;
            """);

            // 2. Tables nothing uses: an old copy of unions (unions are corporations of type Sendika) and empty confederations.
            migrationBuilder.Sql("""
            DROP TABLE IF EXISTS [TradeUnion];
            DROP TABLE IF EXISTS [TradeUnionConfederation];
            """);

            // 3. Data the new keys would reject: 8 early protests (2019) recorded with gender 0, which is no gender (they keep
            //    none; new protests still need one), and 2 cases whose main company no longer exists.
            migrationBuilder.Sql("""
            ALTER TABLE [Protesto] ALTER COLUMN [GenderId] int NULL;
            """);
            migrationBuilder.Sql("""
            UPDATE [Protesto] SET [GenderId] = NULL WHERE [GenderId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Gender] g WHERE g.[Id] = [Protesto].[GenderId]);
            UPDATE [Resistance] SET [MainCompanyId] = NULL WHERE [MainCompanyId] IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Company] c WHERE c.[Id] = [Resistance].[MainCompanyId]);
            """);

            // 4. Column types as the model defines them: audit dates keep the time of day (they were date), names of who
            //    entered a record stay short, a location's place stops being padded to 500 characters (nchar), and corporation
            //    type names may be longer than 25 characters.
            migrationBuilder.Sql("""
            ALTER TABLE [Resistance] ALTER COLUMN [CreateDate] datetime2 NULL;
            ALTER TABLE [Resistance] ALTER COLUMN [UpdateDate] datetime2 NULL;
            ALTER TABLE [Protesto] ALTER COLUMN [CreateDate] datetime2 NULL;
            ALTER TABLE [Protesto] ALTER COLUMN [UpdateDate] datetime2 NULL;
            ALTER TABLE [Resistance] ALTER COLUMN [Creator] nvarchar(50) COLLATE Turkish_CI_AS NULL;
            ALTER TABLE [Resistance] ALTER COLUMN [Updater] nvarchar(50) COLLATE Turkish_CI_AS NULL;
            ALTER TABLE [Protesto] ALTER COLUMN [Creator] nvarchar(50) COLLATE Turkish_CI_AS NULL;
            ALTER TABLE [Protesto] ALTER COLUMN [Updater] nvarchar(50) COLLATE Turkish_CI_AS NULL;
            ALTER TABLE [ProtestoLocation] ALTER COLUMN [Place] nvarchar(500) COLLATE Turkish_CI_AS NULL;
            ALTER TABLE [CorporationType] ALTER COLUMN [Name] nvarchar(100) COLLATE Turkish_CI_AS NULL;
            """);
            migrationBuilder.Sql("""
            UPDATE [ProtestoLocation] SET [Place] = NULLIF(RTRIM([Place]), N'') WHERE [Place] IS NOT NULL;
            """);

            // 5. The model's foreign keys, checked against the data. Only an owner takes its parts along on delete (a case its
            //    protests and links, a protest its links and locations, a news item its case links); a list value (category,
            //    city, corporation, company …) cannot be deleted while something uses it. Before: deleting a category deleted
            //    its cases, deleting a company type its companies; and the corporation type pointed at the company types.
            migrationBuilder.Sql("""
            ALTER TABLE [Company] WITH CHECK ADD CONSTRAINT [FK_Company_CompanyScale_CompanyScaleId] FOREIGN KEY ([CompanyScaleId]) REFERENCES [CompanyScale] ([Id]);
            ALTER TABLE [Company] WITH CHECK ADD CONSTRAINT [FK_Company_CompanyType_CompanyTypeId] FOREIGN KEY ([CompanyTypeId]) REFERENCES [CompanyType] ([Id]);
            ALTER TABLE [Company] WITH CHECK ADD CONSTRAINT [FK_Company_CompanyWorkLine_CompanyWorkLineId] FOREIGN KEY ([CompanyWorkLineId]) REFERENCES [CompanyWorkLine] ([Id]);
            ALTER TABLE [CompanyOutsourceCompany] WITH CHECK ADD CONSTRAINT [FK_CompanyOutsourceCompany_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Company] ([Id]);
            ALTER TABLE [CompanyOutsourceCompany] WITH CHECK ADD CONSTRAINT [FK_CompanyOutsourceCompany_Company_OutsourceCompanyId] FOREIGN KEY ([OutsourceCompanyId]) REFERENCES [Company] ([Id]);
            ALTER TABLE [Corporation] WITH CHECK ADD CONSTRAINT [FK_Corporation_CorporationType_CorporationTypeId] FOREIGN KEY ([CorporationTypeId]) REFERENCES [CorporationType] ([Id]);
            ALTER TABLE [District] WITH CHECK ADD CONSTRAINT [FK_District_City_CityId] FOREIGN KEY ([CityId]) REFERENCES [City] ([Id]);
            ALTER TABLE [Protesto] WITH CHECK ADD CONSTRAINT [FK_Protesto_Gender_GenderId] FOREIGN KEY ([GenderId]) REFERENCES [Gender] ([Id]);
            ALTER TABLE [Protesto] WITH CHECK ADD CONSTRAINT [FK_Protesto_ProtestoEmployeeCount_ProtestoEmployeeCountId] FOREIGN KEY ([ProtestoEmployeeCountId]) REFERENCES [ProtestoEmployeeCount] ([Id]);
            ALTER TABLE [Protesto] WITH CHECK ADD CONSTRAINT [FK_Protesto_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [Resistance] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ProtestoCity] WITH CHECK ADD CONSTRAINT [FK_ProtestoCity_City_CityId] FOREIGN KEY ([CityId]) REFERENCES [City] ([Id]);
            ALTER TABLE [ProtestoCity] WITH CHECK ADD CONSTRAINT [FK_ProtestoCity_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [Protesto] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ProtestoDistrict] WITH CHECK ADD CONSTRAINT [FK_ProtestoDistrict_District_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [District] ([Id]);
            ALTER TABLE [ProtestoDistrict] WITH CHECK ADD CONSTRAINT [FK_ProtestoDistrict_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [Protesto] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ProtestoInterventionType] WITH CHECK ADD CONSTRAINT [FK_ProtestoInterventionType_InterventionType_InterventionTypeId] FOREIGN KEY ([InterventionTypeId]) REFERENCES [InterventionType] ([Id]);
            ALTER TABLE [ProtestoInterventionType] WITH CHECK ADD CONSTRAINT [FK_ProtestoInterventionType_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [Protesto] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ProtestoLocation] WITH CHECK ADD CONSTRAINT [FK_ProtestoLocation_City_CityId] FOREIGN KEY ([CityId]) REFERENCES [City] ([Id]);
            ALTER TABLE [ProtestoLocation] WITH CHECK ADD CONSTRAINT [FK_ProtestoLocation_District_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [District] ([Id]);
            ALTER TABLE [ProtestoLocation] WITH CHECK ADD CONSTRAINT [FK_ProtestoLocation_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [Protesto] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ProtestoProtestoPlace] WITH CHECK ADD CONSTRAINT [FK_ProtestoProtestoPlace_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [Protesto] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ProtestoProtestoPlace] WITH CHECK ADD CONSTRAINT [FK_ProtestoProtestoPlace_ProtestoPlace_ProtestoPlaceId] FOREIGN KEY ([ProtestoPlaceId]) REFERENCES [ProtestoPlace] ([Id]);
            ALTER TABLE [ProtestoProtestoType] WITH CHECK ADD CONSTRAINT [FK_ProtestoProtestoType_Protesto_ProtestoId] FOREIGN KEY ([ProtestoId]) REFERENCES [Protesto] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ProtestoProtestoType] WITH CHECK ADD CONSTRAINT [FK_ProtestoProtestoType_ProtestoType_ProtestoTypeId] FOREIGN KEY ([ProtestoTypeId]) REFERENCES [ProtestoType] ([Id]);
            ALTER TABLE [Resistance] WITH CHECK ADD CONSTRAINT [FK_Resistance_Category_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Category] ([Id]);
            ALTER TABLE [Resistance] WITH CHECK ADD CONSTRAINT [FK_Resistance_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Company] ([Id]);
            ALTER TABLE [Resistance] WITH CHECK ADD CONSTRAINT [FK_Resistance_Company_MainCompanyId] FOREIGN KEY ([MainCompanyId]) REFERENCES [Company] ([Id]);
            ALTER TABLE [Resistance] WITH CHECK ADD CONSTRAINT [FK_Resistance_Corporation_CorporationId] FOREIGN KEY ([CorporationId]) REFERENCES [Corporation] ([Id]);
            ALTER TABLE [Resistance] WITH CHECK ADD CONSTRAINT [FK_Resistance_Corporation_TradeUnionId] FOREIGN KEY ([TradeUnionId]) REFERENCES [Corporation] ([Id]);
            ALTER TABLE [Resistance] WITH CHECK ADD CONSTRAINT [FK_Resistance_EmployeeCount_EmployeeCountId] FOREIGN KEY ([EmployeeCountId]) REFERENCES [EmployeeCount] ([Id]);
            ALTER TABLE [Resistance] WITH CHECK ADD CONSTRAINT [FK_Resistance_TradeUnionAuthority_TradeUnionAuthorityId] FOREIGN KEY ([TradeUnionAuthorityId]) REFERENCES [TradeUnionAuthority] ([Id]);
            ALTER TABLE [ResistanceCorporation] WITH CHECK ADD CONSTRAINT [FK_ResistanceCorporation_Corporation_CorporationId] FOREIGN KEY ([CorporationId]) REFERENCES [Corporation] ([Id]);
            ALTER TABLE [ResistanceCorporation] WITH CHECK ADD CONSTRAINT [FK_ResistanceCorporation_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [Resistance] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ResistanceEmploymentType] WITH CHECK ADD CONSTRAINT [FK_ResistanceEmploymentType_EmploymentType_EmploymentTypeId] FOREIGN KEY ([EmploymentTypeId]) REFERENCES [EmploymentType] ([Id]);
            ALTER TABLE [ResistanceEmploymentType] WITH CHECK ADD CONSTRAINT [FK_ResistanceEmploymentType_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [Resistance] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ResistanceNews] WITH CHECK ADD CONSTRAINT [FK_ResistanceNews_News_NewsId] FOREIGN KEY ([NewsId]) REFERENCES [News] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ResistanceNews] WITH CHECK ADD CONSTRAINT [FK_ResistanceNews_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [Resistance] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ResistanceResistanceReason] WITH CHECK ADD CONSTRAINT [FK_ResistanceResistanceReason_Resistance_ResistanceId] FOREIGN KEY ([ResistanceId]) REFERENCES [Resistance] ([Id]) ON DELETE CASCADE;
            ALTER TABLE [ResistanceResistanceReason] WITH CHECK ADD CONSTRAINT [FK_ResistanceResistanceReason_ResistanceReason_ResistanceReasonId] FOREIGN KEY ([ResistanceReasonId]) REFERENCES [ResistanceReason] ([Id]);
            """);

            // 6. The model's indexes (every foreign key column is indexed; News.ArticleId and the "aynı değil" pairs are unique).
            migrationBuilder.Sql("""
            CREATE INDEX [IX_Company_CompanyScaleId] ON [Company] ([CompanyScaleId]);
            CREATE INDEX [IX_Company_CompanyTypeId] ON [Company] ([CompanyTypeId]);
            CREATE INDEX [IX_Company_CompanyWorkLineId] ON [Company] ([CompanyWorkLineId]);
            CREATE INDEX [IX_CompanyOutsourceCompany_CompanyId] ON [CompanyOutsourceCompany] ([CompanyId]);
            CREATE INDEX [IX_CompanyOutsourceCompany_OutsourceCompanyId] ON [CompanyOutsourceCompany] ([OutsourceCompanyId]);
            CREATE INDEX [IX_Corporation_CorporationTypeId] ON [Corporation] ([CorporationTypeId]);
            CREATE UNIQUE INDEX [IX_DefinitionNotDuplicate_Kind_FirstId_SecondId] ON [DefinitionNotDuplicate] ([Kind], [FirstId], [SecondId]);
            CREATE INDEX [IX_District_CityId] ON [District] ([CityId]);
            CREATE UNIQUE INDEX [IX_News_ArticleId] ON [News] ([ArticleId]) WHERE [ArticleId] IS NOT NULL;
            CREATE INDEX [IX_Protesto_GenderId] ON [Protesto] ([GenderId]);
            CREATE INDEX [IX_Protesto_ProtestoEmployeeCountId] ON [Protesto] ([ProtestoEmployeeCountId]);
            CREATE INDEX [IX_Protesto_ResistanceId] ON [Protesto] ([ResistanceId]);
            CREATE INDEX [IX_ProtestoCity_CityId] ON [ProtestoCity] ([CityId]);
            CREATE INDEX [IX_ProtestoCity_ProtestoId] ON [ProtestoCity] ([ProtestoId]);
            CREATE INDEX [IX_ProtestoDistrict_DistrictId] ON [ProtestoDistrict] ([DistrictId]);
            CREATE INDEX [IX_ProtestoDistrict_ProtestoId] ON [ProtestoDistrict] ([ProtestoId]);
            CREATE INDEX [IX_ProtestoInterventionType_InterventionTypeId] ON [ProtestoInterventionType] ([InterventionTypeId]);
            CREATE INDEX [IX_ProtestoInterventionType_ProtestoId] ON [ProtestoInterventionType] ([ProtestoId]);
            CREATE INDEX [IX_ProtestoLocation_CityId] ON [ProtestoLocation] ([CityId]);
            CREATE INDEX [IX_ProtestoLocation_DistrictId] ON [ProtestoLocation] ([DistrictId]);
            CREATE INDEX [IX_ProtestoLocation_ProtestoId] ON [ProtestoLocation] ([ProtestoId]);
            CREATE INDEX [IX_ProtestoProtestoPlace_ProtestoId] ON [ProtestoProtestoPlace] ([ProtestoId]);
            CREATE INDEX [IX_ProtestoProtestoPlace_ProtestoPlaceId] ON [ProtestoProtestoPlace] ([ProtestoPlaceId]);
            CREATE INDEX [IX_ProtestoProtestoType_ProtestoId] ON [ProtestoProtestoType] ([ProtestoId]);
            CREATE INDEX [IX_ProtestoProtestoType_ProtestoTypeId] ON [ProtestoProtestoType] ([ProtestoTypeId]);
            CREATE INDEX [IX_Resistance_CategoryId] ON [Resistance] ([CategoryId]);
            CREATE INDEX [IX_Resistance_CompanyId] ON [Resistance] ([CompanyId]);
            CREATE INDEX [IX_Resistance_CorporationId] ON [Resistance] ([CorporationId]);
            CREATE INDEX [IX_Resistance_EmployeeCountId] ON [Resistance] ([EmployeeCountId]);
            CREATE INDEX [IX_Resistance_MainCompanyId] ON [Resistance] ([MainCompanyId]);
            CREATE INDEX [IX_Resistance_TradeUnionAuthorityId] ON [Resistance] ([TradeUnionAuthorityId]);
            CREATE INDEX [IX_Resistance_TradeUnionId] ON [Resistance] ([TradeUnionId]);
            CREATE INDEX [IX_ResistanceCorporation_CorporationId] ON [ResistanceCorporation] ([CorporationId]);
            CREATE INDEX [IX_ResistanceCorporation_ResistanceId] ON [ResistanceCorporation] ([ResistanceId]);
            CREATE INDEX [IX_ResistanceEmploymentType_EmploymentTypeId] ON [ResistanceEmploymentType] ([EmploymentTypeId]);
            CREATE INDEX [IX_ResistanceEmploymentType_ResistanceId] ON [ResistanceEmploymentType] ([ResistanceId]);
            CREATE INDEX [IX_ResistanceNews_NewsId] ON [ResistanceNews] ([NewsId]);
            CREATE INDEX [IX_ResistanceNews_ResistanceId] ON [ResistanceNews] ([ResistanceId]);
            CREATE INDEX [IX_ResistanceResistanceReason_ResistanceId] ON [ResistanceResistanceReason] ([ResistanceId]);
            CREATE INDEX [IX_ResistanceResistanceReason_ResistanceReasonId] ON [ResistanceResistanceReason] ([ResistanceReasonId]);
            """);

            // 7. The sixteen migrations Baseline replaced are no longer in the code; forget them.
            migrationBuilder.Sql("""
            DELETE FROM [__EFMigrationsHistory] WHERE [MigrationId] < N'20250804134614_Baseline';
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) =>
            throw new System.NotSupportedException("SchemaFixes rebuilds keys and indexes and cleans data; it is not undone. Restore a backup instead.");
    }
}
