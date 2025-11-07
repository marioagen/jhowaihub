using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StepToolDependencyMap : IEntityTypeConfiguration<StepToolDependency>
    {
        public void Configure(EntityTypeBuilder<StepToolDependency> builder)
        {
            builder.ToTable("StepToolDependencies");

            builder.HasKey(std => std.Id);

            builder.HasOne(std => std.StepTool)
                   .WithMany(st => st.Dependencies) 
                   .HasForeignKey(std => std.StepToolId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(std => std.DependsOnStepTool)
                   .WithMany()
                   .HasForeignKey(std => std.DependsOnStepToolId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.Created)
                   .HasColumnType("datetime")
                   .IsRequired();

            // Ensure unique constraint on StepToolId + DependsOnStepToolId
            builder.HasIndex(std => new { std.StepToolId, std.DependsOnStepToolId })
                   .IsUnique();
        }
    }
}
