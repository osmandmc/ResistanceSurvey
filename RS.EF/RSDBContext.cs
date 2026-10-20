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
        public DbSet<ProtestoEmployeeCount> ProtestoEmployeeCount { get; set; }
        public DbSet<ProtestoProtestoPlace> ProtestoProtestoPlace { get; set; }
        public DbSet<ResistanceResistanceReason> ResistanceResistanceReason  { get; set; }
        public DbSet<ProtestoProtestoType> ProtestoProtestoType { get; set; }
        public DbSet<ProtestoInterventionType> ProtestoInterventionType { get; set; }
        public DbSet<TradeUnionAuthority> TradeUnionAuthority { get; set; }
        public DbSet<ProtestoCity> ProtestoCity { get; set; }
        public DbSet<ProtestoDistrict> ProtestoDistrict { get; set; }
        public DbSet<DefinitionNotDuplicate> DefinitionNotDuplicate { get; set; }

       
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
                     var ownsIt = owner == typeof(Resistance) || owner == typeof(Protesto)
                         || (owner == typeof(News) && foreignKey.DeclaringEntityType.ClrType == typeof(ResistanceNews));
                     foreignKey.DeleteBehavior = ownsIt ? DeleteBehavior.Cascade : DeleteBehavior.Restrict;
                 }

        }
    }
}
