using Api.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.EntityTypeConfigurations
{
    public class SpoolerEntityTypeConfiguration : IEntityTypeConfiguration<Spooler>
    {
        public void Configure(EntityTypeBuilder<Spooler> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => x.Key).IsUnique();
        }
    }
}
