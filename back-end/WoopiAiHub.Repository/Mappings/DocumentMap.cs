using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class DocumentMap : IEntityTypeConfiguration<Document>
    {
        private const string TeamIdColumn = "TeamId";
        private const string DocumentIdColumn = "DocumentId";

        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                   .IsRequired()
                   .HasMaxLength(251);

            builder.Property(u => u.Description)
                   .HasMaxLength(250);

            builder.Property(u => u.ReferenceFile)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(u => u.Status)
                   .IsRequired();

            builder.Property(u => u.EmailCreator)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.Created)
                   .IsRequired();

            builder.Property(u => u.HasBatch)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasMany(u => u.DocumentHistories)
                   .WithOne(s => s.Document)
                   .HasForeignKey(c => c.IdDocument);

            builder.HasOne(u => u.DocumentNormalized)
                    .WithOne(s => s.Document)
                    .HasForeignKey<DocumentNormalized>(c => c.IdDocument);

            builder.HasMany(u => u.Cards)
                   .WithOne(s => s.Document)
                   .HasForeignKey(c => c.DocumentId);
        }
    }
}
