using Api.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class CloudspoolContext : DbContext
    {
        public DbSet<Document> Document { get; set; }
        public DbSet<Format> Format { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Resource> Resource { get; set; }
        public DbSet<Spooler> Spooler { get; set; }
        public DbSet<Template> Template { get; set; }
        public DbSet<Terminal> Terminal { get; set; }
        public DbSet<Zone> Zone { get; set; }

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
