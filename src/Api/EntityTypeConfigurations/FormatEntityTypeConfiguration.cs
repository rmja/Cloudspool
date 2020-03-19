using Api.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.EntityTypeConfigurations
{
    public class FormatEntityTypeConfiguration : IEntityTypeConfiguration<Format>
    {
        public void Configure(EntityTypeBuilder<Format> builder)
        {
            builder.Property(x => x.Alias).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => new { x.ZoneId, x.Alias }).IsUnique();
        }
    }
}
