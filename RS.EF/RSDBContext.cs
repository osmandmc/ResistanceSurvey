using System;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore;
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
        public DbSet<Company> Company { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<CompanyScale> CompanyScale { get; set; }
        public DbSet<CompanyType> CompanyType { get; set; }
        public DbSet<CompanyWorkLine> CompanyWorkLine { get; set; }
        public DbSet<Corporation> Corporation { get; set; }
        public DbSet<District> District { get; set; }
        public DbSet<EmployeeCount> EmployeeCount { get; set; }
        public DbSet<EmploymentType> EmploymentType { get; set; }
        public DbSet<Gender> Gender { get; set; }
        public DbSet<InterventionType> InterventionType { get; set; }
        public DbSet<ProtestoPlace> ProtestoPlace { get; set; }
        public DbSet<ProtestoReason> ProtestoReason { get; set; }
        public DbSet<ProtestoType> ProtestoType { get; set; }
        
        public DbSet<ProtestoProtestoPlace> ProtestoProtestoPlace { get; set; }
        public DbSet<ProtestoProtestoReason> ProtestoProtestoReason { get; set; }
        public DbSet<ProtestoProtestoType> ProtestoProtestoType { get; set; }
        public DbSet<ProtestoInterventionType> ProtestoInterventionType { get; set; }
        public DbSet<TradeUnion> TradeUnion { get; set; }
        public DbSet<TradeUnionAuthority> TradeUnionAuthority { get; set; }
        public DbSet<TradeUnionConfederation> TradeUnionConfederation { get; set; }

       
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     optionsBuilder.UseMySQL(@"Server=127.0.0.1; Uid=root; Pwd=root; Database=RSDB;");
            
        // }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resistance>()
                .HasKey(b => b.Id)
                .HasName("PrimaryKey_ResistanceId");
        }
    }
}
