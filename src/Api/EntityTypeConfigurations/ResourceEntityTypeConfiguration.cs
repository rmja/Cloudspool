using Api.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.EntityTypeConfigurations
{
    public class ResourceEntityTypeConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.Property(x => x.Alias).HasMaxLength(100).IsRequired();
            builder.Property(x => x.MediaType).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => new { x.ProjectId, x.Alias }).IsUnique();
        }
    }
}
