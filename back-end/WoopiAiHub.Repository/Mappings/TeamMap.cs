using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WoopiAiHub.Repository.Mappings
{
    public class TeamMap : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.ToTable("Teams");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                   .HasColumnName("Name")
                   .HasColumnType("varchar(100)")
                   .IsRequired();

            builder.HasMany(t => t.Workflows)
                   .WithMany(w => w.Teams)
                   .UsingEntity<Dictionary<string, object>>(
                        "WorkflowTeams",
                        j => j.HasOne<Workflow>().WithMany().HasForeignKey("WorkflowId"),
                        j => j.HasOne<Team>().WithMany().HasForeignKey("TeamId"),
                        j =>
                        {
                            j.HasKey("WorkflowId", "TeamId");
                            j.ToTable("WorkflowTeams");
                        }
                   );
        }
    }
}
