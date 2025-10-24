using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StepMap : IEntityTypeConfiguration<Step>
    {
        public void Configure(EntityTypeBuilder<Step> builder)
        {
            builder.ToTable("Steps");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(s => s.Order)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(s => s.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasOne(w => w.Workflow)
                .WithMany(w => w.Steps)
                .HasForeignKey(w => w.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Cards)
                .WithOne(c => c.Step)
                .HasForeignKey(c => c.StepId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Profile)
                .WithMany(p => p.Steps)
                .HasForeignKey(s => s.ProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Status)
                .WithMany(s => s.Steps)
                .HasForeignKey(s => s.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => new { s.WorkflowId, s.Order }).IsUnique();
            builder.HasIndex(s => s.Name);
            builder.HasIndex(s => s.Created);
        }
    }
}