using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class WorkflowMap : IEntityTypeConfiguration<Workflow>
    {
        private const string WorkflowIdColumn = "WorkflowId";
        private const string DocumentIdColumn = "DocumentId";
        public void Configure(EntityTypeBuilder<Workflow> builder)
        {
            builder.ToTable("Workflows");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(w => w.Enable)
               .HasColumnName("Enable")
               .HasColumnType("bit")
               .IsRequired();

            builder.Property(w => w.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasMany(w => w.Steps)
                .WithOne(s => s.Workflow)
                .HasForeignKey(s => s.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.Documents)
                .WithMany(d => d.Workflows)
                .UsingEntity<Dictionary<string, object>>(
                    "WorkflowDocuments",
                    j => j.HasOne<Document>()
                          .WithMany()
                          .HasForeignKey(DocumentIdColumn)
                          .OnDelete(DeleteBehavior.Restrict),

                    j => j.HasOne<Workflow>()
                          .WithMany()
                          .HasForeignKey(WorkflowIdColumn)
                          .OnDelete(DeleteBehavior.Restrict),

                    j =>
                    {
                        j.HasKey(WorkflowIdColumn, DocumentIdColumn);
                        j.ToTable("WorkflowDocuments");

                        j.Property<int>(WorkflowIdColumn).HasColumnName(WorkflowIdColumn);
                        j.Property<int>(DocumentIdColumn).HasColumnName(DocumentIdColumn);
                    }
                );

            builder.HasIndex(w => w.Created);
        }
    }
}
