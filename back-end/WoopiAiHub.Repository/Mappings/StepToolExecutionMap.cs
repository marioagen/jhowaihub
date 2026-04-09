using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StepToolExecutionMap : IEntityTypeConfiguration<StepToolExecution>
    {
        public void Configure(EntityTypeBuilder<StepToolExecution> builder)
        {
            builder.ToTable("StepToolExecutions");

            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.StepTool)
                   .WithMany(st => st.Executions)
                   .HasForeignKey(e => e.StepToolId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Card)
                   .WithMany(c => c.Executions)
                   .HasForeignKey(e => e.CardId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.Started)
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.Property(e => e.Completed)
                   .HasColumnType("datetime")
                   .IsRequired(false);

            builder.Property(e => e.Status)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(p => p.Created)
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.HasIndex(e => new { e.StepToolId, e.CardId})
                   .HasDatabaseName("IX_StepToolExecution_StepToolId_CardId");
        }
    }
}
