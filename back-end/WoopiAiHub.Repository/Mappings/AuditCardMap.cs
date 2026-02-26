using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models.Audit;

namespace WoopiAiHub.Repository.Mappings
{
    public class AuditCardMap : IEntityTypeConfiguration<AuditCard>
    {
        public void Configure(EntityTypeBuilder<AuditCard> builder)
        {
            builder.ToTable("AuditCards");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.ActionType)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(c => c.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(c => c.OccurredAt)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasOne(a => a.Card)
                .WithMany()
                .HasForeignKey(a => a.CardId)
                .IsRequired();

            builder.HasOne(a => a.Workflow)
                .WithMany()
                .HasForeignKey(a => a.WorkflowId)
                .IsRequired();

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .IsRequired();

            builder.HasIndex(c => c.CardId);
            builder.HasIndex(c => c.WorkflowId);
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.ActionType);
            builder.HasIndex(c => c.OccurredAt);
        }
    }
}