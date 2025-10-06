using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StepToolParameterMap : IEntityTypeConfiguration<StepToolParameter>
    {
        public void Configure(EntityTypeBuilder<StepToolParameter> builder)
        {
            builder.ToTable("StepToolParameters");

            builder.HasKey(p => p.Id);

            builder.HasOne(e => e.StepTool)
                   .WithMany(u => u.Parameters) 
                   .HasForeignKey(p => p.StepToolId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(p => p.Value)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired();

            builder.Property(p => p.RequiredFile)
                   .IsRequired();


            builder.Property(p => p.WebhookId)
                   .IsRequired(false);

            builder.Property(p => p.Created)
                   .HasColumnType("datetime")
                   .IsRequired();
        }
    }
}
