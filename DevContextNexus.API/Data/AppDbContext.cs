using DevContextNexus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DevContextNexus.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ContextFile> ContextFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
