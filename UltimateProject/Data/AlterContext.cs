using Microsoft.EntityFrameworkCore;
using UltimateProject.Models;

namespace UltimateProject.Data
{
    public class AlterContext : DbContext
    {
        public DbSet<MainEntity> MainEntities { get; set; }
        public DbSet<RelatedEntity> RelatedEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=alter_database.db");
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MainEntity>()
                .HasMany(m => m.RelatedEntities)
                .WithMany(r => r.MainEntities)
                .UsingEntity(j => j.ToTable("MainEntityRelatedEntity"));
        }
    }
}