using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StepToolMap : IEntityTypeConfiguration<StepTool>
    {
        public void Configure(EntityTypeBuilder<StepTool> builder)
        {
            builder.ToTable("StepTools");

            builder.HasKey(st => st.Id);

            builder.HasOne(st => st.Step)
                   .WithMany(s => s.StepTools) 
                   .HasForeignKey(st => st.StepId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(st => st.Tool)
                   .WithMany(t => t.StepTools) 
                   .HasForeignKey(st => st.ToolId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(st => st.DependsOnStepTool)
                   .WithOne() 
                   .HasForeignKey<StepTool>(st => st.DependsOnStepToolId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(st => st.Outputs)
                   .WithOne(o => o.StepTool)
                   .HasForeignKey(o => o.StepToolId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(st => st.Order)
                   .HasColumnName("StepOrder")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(st => st.PositionX)
                   .HasColumnName("PositionX")
                   .HasColumnType("decimal(9,2)")
                   .IsRequired();

            builder.Property(st => st.PositionY)
                   .HasColumnName("PositionY")
                   .HasColumnType("decimal(9,2)")
                   .IsRequired();

            builder.Property(p => p.Created)
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.HasIndex(st => st.DependsOnStepToolId)
                   .IsUnique()
                   .HasFilter("[DependsOnStepToolId] IS NOT NULL");
        }
    }
}
