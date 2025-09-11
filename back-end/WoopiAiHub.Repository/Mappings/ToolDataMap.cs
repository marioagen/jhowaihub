using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class ToolDataMap : IEntityTypeConfiguration<ToolData>
    {
        public void Configure(EntityTypeBuilder<ToolData> builder)
        {
            builder.ToTable("ToolDatas");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .HasColumnName("Name")
                   .HasColumnType("varchar(50)")
                   .IsRequired();

            builder.Property(t => t.IsActive)
                   .HasColumnName("IsActive")
                   .HasColumnType("bit")
                   .IsRequired();

            builder.HasMany(td => td.Tools)
                   .WithOne(t => t.InputData)
                   .HasForeignKey(t => t.InputDataId);
        }
    }
}
