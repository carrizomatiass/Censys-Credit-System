using CreditSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditSystem.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<CreditApplication> CreditApplications => Set<CreditApplication>();
        public DbSet<ProcessStep> ProcessSteps => Set<ProcessStep>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CreditApplication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RequestedAmount).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasConversion<int>();

                entity.HasMany(e => e.Steps)
                      .WithOne()
                      .HasForeignKey(s => s.CreditApplicationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Navigation(e => e.Steps)
                      .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<ProcessStep>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StepType).HasConversion<int>();
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            });
        }
    }
}
