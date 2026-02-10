using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class ToolTypeMap : IEntityTypeConfiguration<ToolType>
    {
        public void Configure(EntityTypeBuilder<ToolType> builder)
        {
            builder.ToTable("ToolTypes");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .HasColumnName("Name")
                   .HasColumnType("varchar(50)")
                   .IsRequired();

            builder.Property(t => t.Description)
                   .HasColumnName("Description")
                   .HasColumnType("varchar(100)")
                   .IsRequired();

            builder.Property(t => t.IsActive)
                   .HasColumnName("IsActive")
                   .HasColumnType("bit")
                   .IsRequired();

            builder.HasMany(tt => tt.Tools)
                   .WithOne(t => t.ToolType)
                   .HasForeignKey(t => t.ToolTypeId);
        }
    }
}
