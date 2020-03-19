using Api.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class CloudspoolContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<Format> Formats { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Spooler> Spoolers { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Terminal> Terminals { get; set; }
        public DbSet<Zone> Zones { get; set; }

        public CloudspoolContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
            .UseSnakeCaseNamingConvention();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CloudspoolContext).Assembly);

            modelBuilder.UseIdentityColumns();
        }
    }
}
