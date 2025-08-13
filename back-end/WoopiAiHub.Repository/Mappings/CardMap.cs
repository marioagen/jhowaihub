using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class CardMap : IEntityTypeConfiguration<Card>
    {
        public void Configure(EntityTypeBuilder<Card> builder)
        {
            builder.ToTable("Cards");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasOne(c => c.Step)
                .WithMany(s => s.Cards)
                .HasForeignKey(c => c.StepId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Document)
                .WithMany(d => d.Cards)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Status)
                .WithMany(s => s.Cards)
                .HasForeignKey(s => s.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.Name);
            builder.HasIndex(c => c.Created);
        }
    }
}
