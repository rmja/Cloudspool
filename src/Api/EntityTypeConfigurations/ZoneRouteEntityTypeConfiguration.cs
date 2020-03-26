using Api.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.EntityTypeConfigurations
{
    public class ZoneRouteEntityTypeConfiguration : IEntityTypeConfiguration<ZoneRoute>
    {
        public void Configure(EntityTypeBuilder<ZoneRoute> builder)
        {
            builder.Property(x => x.Alias).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => new { x.ZoneId, x.Alias }).IsUnique();
        }
    }
}
