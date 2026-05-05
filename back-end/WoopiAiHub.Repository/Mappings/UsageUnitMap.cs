using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class UsageUnitMap : IEntityTypeConfiguration<UsageUnit>
    {
        public void Configure(EntityTypeBuilder<UsageUnit> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                   .HasColumnName("Name")
                   .HasColumnType("varchar(100)")
                   .IsRequired();

            builder.Property(u => u.UsageTypeId)
                   .HasColumnName("UsageTypeId")
                   .HasColumnType("int");

            builder.Property(u => u.ModelEmbeddingId)
                   .HasColumnName("ModelEmbeddingId")
                   .HasColumnType("int");

            builder.Property(u => u.Created)
                   .HasColumnName("Created")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.Property(u => u.Value)
                   .HasColumnName("Value")
                   .HasColumnType("decimal(18,9)")
                   .IsRequired();

            builder.HasOne(u => u.UsageType)
                   .WithMany(ut => ut.Units)
                   .HasForeignKey(u => u.UsageTypeId);

            builder.HasOne(u => u.ModelEmbedding)
                   .WithMany(ut => ut.UsageUnits)
                   .HasForeignKey(u => u.ModelEmbeddingId);
        }
    }
}
