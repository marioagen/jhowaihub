using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class DocumentAnalysisRejectionMap : IEntityTypeConfiguration<DocumentAnalysisRejection>
    {
        public void Configure(EntityTypeBuilder<DocumentAnalysisRejection> builder)
        {
            builder.ToTable("DocumentAnalysisRejections");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Justification)
                .HasColumnType("nvarchar(MAX)")
                .IsRequired();

            builder.Property(d => d.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasOne(d => d.Card)
                .WithMany()
                .HasForeignKey(d => d.CardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Step)
                .WithMany()
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(d => d.CardId);
            builder.HasIndex(d => d.Created);
        }
    }
}
