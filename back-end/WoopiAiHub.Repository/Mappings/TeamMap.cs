using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WoopiAiHub.Repository.Mappings
{
    public class TeamMap : IEntityTypeConfiguration<Team>
    {
        private const string WorkflowIdColumn = "WorkflowId";
        private const string TeamIdColumn = "TeamId";
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
                        j => j.HasOne<Workflow>().WithMany().HasForeignKey(WorkflowIdColumn),
                        j => j.HasOne<Team>().WithMany().HasForeignKey(TeamIdColumn),
                        j =>
                        {
                            j.HasKey(WorkflowIdColumn, TeamIdColumn);
                            j.ToTable("WorkflowTeams");
                            j.Property<int>(WorkflowIdColumn).HasColumnName(WorkflowIdColumn);
                            j.Property<int>(TeamIdColumn).HasColumnName(TeamIdColumn);
                        }
                   );
        }
    }
}
