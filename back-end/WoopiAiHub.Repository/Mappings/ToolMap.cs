using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class ToolMap : IEntityTypeConfiguration<Tool>
    {
        public void Configure(EntityTypeBuilder<Tool> builder)
        {
            builder.ToTable("Tools");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .HasColumnName("Name")
                   .HasColumnType("varchar(100)")
                   .IsRequired();

            builder.Property(t => t.IsActive)
                   .HasColumnName("IsActive")
                   .HasColumnType("bit")
                   .IsRequired();

            builder.HasOne(t => t.ToolType)
                   .WithMany(tt => tt.Tools)
                   .HasForeignKey(t => t.ToolTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.InputData)
                   .WithMany(td => td.Tools)
                   .HasForeignKey(t => t.InputDataId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.OutputData)
                   .WithMany(td => td.Tools)
                   .HasForeignKey(t => t.OutputDataId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
