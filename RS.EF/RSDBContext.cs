using System;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore;
using RS.COMMON.Entities;

namespace RS.EF
{
    public class RSDBContext : DbContext
    {
        public DbSet<Resistance> Resistance { get; set; }
       
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(@"Server=127.0.0.1; Uid=root; Pwd=root; Database=RSDB;");
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resistance>()
                .HasKey(b => b.Id)
                .HasName("PrimaryKey_ResistanceId");
        }
    }
}
