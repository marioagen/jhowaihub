using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class WorkflowMap : IEntityTypeConfiguration<Workflow>
    {
        public void Configure(EntityTypeBuilder<Workflow> builder)
        {
            builder.ToTable("Workflows");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(w => w.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasMany(w => w.Steps)
                .WithOne(s => s.Workflow)
                .HasForeignKey(s => s.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.Documents)
                .WithMany(d => d.Workflow)
                .UsingEntity<Dictionary<string, object>>(
                    "WorkflowDocuments",
                    j => j.HasOne<Document>()
                          .WithMany()
                          .HasForeignKey("DocumentId")
                          .OnDelete(DeleteBehavior.Restrict),

                    j => j.HasOne<Workflow>()
                          .WithMany()
                          .HasForeignKey("WorkflowId")
                          .OnDelete(DeleteBehavior.Restrict),

                    j =>
                    {
                        j.HasKey("WorkflowId", "DocumentId");
                        j.ToTable("WorkflowDocuments");

                        j.Property<int>("WorkflowId").HasColumnName("WorkflowId");
                        j.Property<int>("DocumentId").HasColumnName("DocumentId");
                    }
                );

            builder.HasIndex(w => w.Created);
        }
    }
}
