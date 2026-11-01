using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RS.COMMON.Entities;
using RS.COMMON.Entities.LookupEntity;

namespace RS.EF
{
    public class RSDBContext : DbContext
    {
        public RSDBContext(DbContextOptions options)
        : base(options)
        {

        }
        public DbSet<Resistance> Resistance { get; set; }
        public DbSet<ResistanceCorporation> ResistanceCorporation { get; set; }
        public DbSet<ResistanceEmploymentType> ResistanceEmploymentType { get; set; }
        public DbSet<CompanyOutsourceCompany> CompanyOutsourceCompany { get; set; }
        public DbSet<Protesto> Protesto { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<CompanyScale> CompanyScale { get; set; }
        public DbSet<CompanyType> CompanyType { get; set; }
        public DbSet<CompanyWorkLine> CompanyWorkLine { get; set; }
        public DbSet<Corporation> Corporation { get; set; }
        public DbSet<CorporationType> CorporationType { get; set; }
        public DbSet<District> District { get; set; }
        public DbSet<EmployeeCount> EmployeeCount { get; set; }
        public DbSet<EmploymentType> EmploymentType { get; set; }
        public DbSet<Gender> Gender { get; set; }
        public DbSet<InterventionType> InterventionType { get; set; }
        public DbSet<ProtestoPlace> ProtestoPlace { get; set; }
        public DbSet<ResistanceReason> ResistanceReason { get; set; }
        public DbSet<ProtestoType> ProtestoType { get; set; }
        public DbSet<ResistanceNews> ResistanceNews { get; set; }

        public DbSet<ProtestoLocation> ProtestoLocation { get; set; }
        public DbSet<Workplace> Workplace { get; set; }
        public DbSet<CompanyRelation> CompanyRelation { get; set; }
        public DbSet<ProtestoEmployeeCount> ProtestoEmployeeCount { get; set; }
        public DbSet<ProtestoProtestoPlace> ProtestoProtestoPlace { get; set; }
        public DbSet<ResistanceResistanceReason> ResistanceResistanceReason  { get; set; }
        public DbSet<ProtestoProtestoType> ProtestoProtestoType { get; set; }
        public DbSet<ProtestoInterventionType> ProtestoInterventionType { get; set; }
        public DbSet<TradeUnionAuthority> TradeUnionAuthority { get; set; }
        public DbSet<ProtestoCity> ProtestoCity { get; set; }
        public DbSet<ProtestoDistrict> ProtestoDistrict { get; set; }
        public DbSet<DefinitionNotDuplicate> DefinitionNotDuplicate { get; set; }
        public DbSet<AiSuggestion> AiSuggestion { get; set; }
        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<ActivityEntry> ActivityEntry { get; set; }
        public DbSet<CaseMigrationRecord> CaseMigrationRecord { get; set; }

       
        public const string TurkishCollation = "Turkish_CI_AS";

        // All text follows Turkish rules, so "işçi" finds "İŞÇİ" and lists sort as in Turkish (F9). The database
        // default (SQL_Latin1_General_CP1_CI_AS) ignores case only for Latin letters.
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.Properties<string>().UseCollation(TurkishCollation);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

                 modelBuilder.Entity<Company>()
                    .HasMany(u => u.OutsourceCompanies).WithOne(u => u.Company)
                    .OnDelete(DeleteBehavior.Restrict);

                 // Pipeline article id: SHA-256 hex of the canonical URL. NULL for news from before the cut-over.
                 modelBuilder.Entity<News>()
                    .Property(n => n.ArticleId)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                 modelBuilder.Entity<News>()
                    .HasIndex(n => n.ArticleId)
                    .IsUnique()
                    .HasFilter("[ArticleId] IS NOT NULL");

                 modelBuilder.Entity<DefinitionNotDuplicate>(e =>
                 {
                     e.Property(x => x.Kind).HasMaxLength(32).IsRequired();
                     e.HasIndex(x => new { x.Kind, x.FirstId, x.SecondId }).IsUnique();
                 });

                 // "AI ile doldur" (R19c): one per article.
                 modelBuilder.Entity<AiSuggestion>(e =>
                 {
                     e.HasIndex(x => x.NewsId).IsUnique();
                     e.Property(x => x.Origin).HasMaxLength(100).IsRequired();
                     e.Property(x => x.PromptVersion).HasMaxLength(20);
                     e.Property(x => x.CreatedBy).HasMaxLength(50);
                 });

                 // Accounts (F4a). Hashes are compared exactly, so they keep the database default collation.
                 modelBuilder.Entity<AppUser>(e =>
                 {
                     e.Property(x => x.Username).HasMaxLength(50).IsRequired();
                     e.HasIndex(x => x.Username).IsUnique();
                     e.Property(x => x.Role).HasMaxLength(20).IsRequired();
                     e.Property(x => x.PasswordHash).HasMaxLength(200).IsUnicode(false).UseCollation(null);
                     e.Property(x => x.InviteTokenHash).HasMaxLength(64).IsUnicode(false).UseCollation(null);
                     e.HasIndex(x => x.InviteTokenHash).IsUnique().HasFilter("[InviteTokenHash] IS NOT NULL");
                     e.Property(x => x.SecurityStamp).HasMaxLength(64).IsUnicode(false).UseCollation(null).IsRequired();
                     e.Property(x => x.CreatedBy).HasMaxLength(50);
                 });

                 // Activity log (F4b): read newest first, by case, by person.
                 modelBuilder.Entity<ActivityEntry>(e =>
                 {
                     e.Property(x => x.Username).HasMaxLength(50);
                     e.Property(x => x.Action).HasMaxLength(40).IsUnicode(false).UseCollation(null).IsRequired();
                     e.Property(x => x.ArticleId).HasMaxLength(64).IsUnicode(false).UseCollation(null);
                     e.Property(x => x.Subject).HasMaxLength(40).IsUnicode(false).UseCollation(null);
                     e.Property(x => x.Summary).HasMaxLength(500);
                     e.HasIndex(x => x.At);
                     e.HasIndex(x => x.CaseId);
                     e.HasIndex(x => new { x.Username, x.At });
                 });

                 // R31: the record of what the case-category migration changed; one or more rows per case.
                 modelBuilder.Entity<CaseMigrationRecord>(e =>
                 {
                     e.Property(x => x.Step).HasMaxLength(30).IsUnicode(false).UseCollation(null).IsRequired();
                     e.Property(x => x.RemovedCorporationIds).HasMaxLength(200).IsUnicode(false).UseCollation(null);
                     e.HasIndex(x => x.ResistanceId);
                 });

                 // R31: a solidarity case points at the case it supported; deleting either never touches the other.
                 modelBuilder.Entity<Resistance>()
                    .HasOne(r => r.SupportedResistance).WithMany()
                    .HasForeignKey(r => r.SupportedResistanceId);

                 // R34: unions and confederations share the organization table; Kind says which, and follows the type.
                 modelBuilder.Entity<Corporation>()
                    .HasDiscriminator<OrganizationKind>("Kind")
                    .HasValue<Corporation>(OrganizationKind.Organization)
                    .HasValue<TradeUnion>(OrganizationKind.TradeUnion)
                    .HasValue<Confederation>(OrganizationKind.Confederation);
                 modelBuilder.Entity<TradeUnion>()
                    .HasOne(u => u.Confederation).WithMany(c => c.Unions)
                    .HasForeignKey(u => u.ConfederationId);

                 // R37: a workplace (a place, not a company); its name as long as a company's.
                 modelBuilder.Entity<Workplace>(e =>
                 {
                     e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                     e.HasIndex(x => x.Name);
                 });

                 // R37, part C: a company's two branches come from the same list (işkolu, or hizmet kolu for civil servants).
                 modelBuilder.Entity<Company>()
                    .HasOne(c => c.ServiceBranch).WithMany()
                    .HasForeignKey(c => c.ServiceBranchId);

                 // R37, part B: facts between two companies (group, public body, new name).
                 modelBuilder.Entity<CompanyRelation>(e =>
                 {
                     e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
                     e.HasOne(x => x.RelatedCompany).WithMany().HasForeignKey(x => x.RelatedCompanyId);
                     e.Property(x => x.SharePercent).HasPrecision(5, 2);
                     e.Property(x => x.SourceUrl).HasMaxLength(500);
                     e.Property(x => x.Note).HasMaxLength(500);
                     e.Property(x => x.CreatedBy).HasMaxLength(50);
                 });

                 // A hash in hex: compared exactly, kept in the database default collation (see ConfigureConventions).
                 modelBuilder.Entity<News>().Property(n => n.ArticleId).UseCollation(null);

                 // Lengths as the database has them (F12): who entered or changed a case or protest; a protest location's place.
                 foreach (var audited in new[] { typeof(Resistance), typeof(Protesto) })
                 {
                     modelBuilder.Entity(audited).Property(nameof(AuditEntity.Creator)).HasMaxLength(50);
                     modelBuilder.Entity(audited).Property(nameof(AuditEntity.Updater)).HasMaxLength(50);
                 }
                 modelBuilder.Entity<ProtestoLocation>().Property(l => l.Place).HasMaxLength(500);
                 modelBuilder.Entity<CorporationType>().Property(t => t.Name).HasMaxLength(100);

                 // Delete rules (F12): a case or protest takes its own parts along (protests, links, locations); a list value
                 // (category, city, corporation, company …) can only be deleted when nothing uses it. Before, deleting a
                 // category deleted all its cases, and deleting a company type deleted its companies.
                 foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                 {
                     var owner = foreignKey.PrincipalEntityType.ClrType;
                     var part = foreignKey.DeclaringEntityType.ClrType;
                     // An AI suggestion belongs to its article, not to the case it went to (cases are only marked deleted).
                     var ownsIt = (owner == typeof(Resistance) && part != typeof(AiSuggestion) && part != typeof(Resistance)) || owner == typeof(Protesto)
                         || (owner == typeof(News) && (part == typeof(ResistanceNews) || part == typeof(AiSuggestion)));
                     foreignKey.DeleteBehavior = ownsIt ? DeleteBehavior.Cascade : DeleteBehavior.Restrict;
                 }

        }
    }
}
