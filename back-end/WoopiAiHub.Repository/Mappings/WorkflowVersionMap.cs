using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class WorkflowVersionMap : IEntityTypeConfiguration<WorkflowVersion>
    {
        public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
        {
            builder.ToTable("WorkflowVersions");

            builder.HasKey(wv => wv.Id);

            builder.Property(wv => wv.WorkflowId)
                .HasColumnName("WorkflowId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(wv => wv.VersionNumber)
                .HasColumnName("VersionNumber")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(wv => wv.ConfigSnapshot)
                .HasColumnName("ConfigSnapshot")
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(wv => wv.TriggerToolId)
                .HasColumnName("TriggerToolId")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(wv => wv.TriggerToolName)
                .HasColumnName("TriggerToolName")
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(wv => wv.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasOne(wv => wv.Workflow)
                .WithMany(w => w.Versions)
                .HasForeignKey(wv => wv.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(wv => wv.WorkflowId);
        }
    }
}
