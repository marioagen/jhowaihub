using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class DocumentAnonymizationMap : IEntityTypeConfiguration<DocumentAnonymization>
    {
        public void Configure(EntityTypeBuilder<DocumentAnonymization> builder)
        {
            builder.ToTable("DocumentAnonymizations");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.DocumentUrl)
                   .IsRequired();

            builder.HasOne(u => u.Document)
                   .WithMany(s => s.DocumentAnonymizations)
                   .HasForeignKey(c => c.DocumentId);
        }
    }
}
