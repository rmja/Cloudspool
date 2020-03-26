using Api.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.EntityTypeConfigurations
{
    public class TerminalRouteEntityTypeConfiguration : IEntityTypeConfiguration<TerminalRoute>
    {
        public void Configure(EntityTypeBuilder<TerminalRoute> builder)
        {
            builder.Property(x => x.Alias).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => new { x.TerminalId, x.Alias }).IsUnique();
        }
    }
}
