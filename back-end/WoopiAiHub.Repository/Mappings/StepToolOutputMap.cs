using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StepToolOutputMap : IEntityTypeConfiguration<StepToolOutput>
    {
        public void Configure(EntityTypeBuilder<StepToolOutput> builder)
        {
            builder.ToTable("StepToolOutputs");

            builder.HasKey(o => o.Id);

            builder.HasOne<StepTool>()
                   .WithMany() 
                   .HasForeignKey(o => o.StepToolId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Card>()
                   .WithMany() 
                   .HasForeignKey(o => o.CardId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(o => o.Value)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired();

            builder.Property(o => o.Created)
                   .HasColumnType("datetime")
                   .IsRequired();
        }
    }
}
