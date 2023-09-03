using Microsoft.EntityFrameworkCore;
using Moonlapse.Server.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Data.DbContexts {
    public class MoonlapseDbContext : DbContext {
        public DbSet<User> Users { get; set; }

        public MoonlapseDbContext(DbContextOptions<MoonlapseDbContext> options) : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();
        }
    }
}
