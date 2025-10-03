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

            builder.HasMany(w => w.Teams)
                .WithMany(t => t.Workflows)
                .UsingEntity<Dictionary<string, object>>(
                    "WorkflowTeams",
                    j => j.HasOne<Team>().WithMany().HasForeignKey("TeamId"),
                    j => j.HasOne<Workflow>().WithMany().HasForeignKey("WorkflowId"),
                    j =>
                    {
                        j.HasKey("WorkflowId", "TeamId");
                        j.ToTable("WorkflowTeams");
                    }
                );

            builder.HasIndex(w => w.TeamId);
            builder.HasIndex(w => w.Created);
        }
    }
}
