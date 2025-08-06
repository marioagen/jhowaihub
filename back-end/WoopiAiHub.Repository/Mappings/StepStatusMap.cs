using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StepStatusMap : IEntityTypeConfiguration<StepStatus>
    {
        public void Configure(EntityTypeBuilder<StepStatus> builder)
        {
            builder.ToTable("StepStatus");

            builder.HasKey(ss => ss.Id);

            builder.Property(ss => ss.Name)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(ss => ss.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasIndex(ss => ss.Name).IsUnique();
            builder.HasIndex(ss => ss.Created);
        }
    }
}
