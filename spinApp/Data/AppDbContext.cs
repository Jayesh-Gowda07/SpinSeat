using spinApp.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;


namespace spinApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<DailyNumber> DailyNumbers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DailyNumber>()
                .HasIndex(dn => new { dn.Date, dn.Number })
                .IsUnique();

            modelBuilder.Entity<DailyNumber>()
                .HasIndex(dn => new { dn.UserId, dn.Date })
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Name)
                .IsUnique();
        }
    }
}
