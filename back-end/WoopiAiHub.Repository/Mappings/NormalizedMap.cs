using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;


namespace WoopiAiHub.Repository.Mappings
{
    public class DocumentNormalizedMap
    {
        public void Configure(EntityTypeBuilder<DocumentNormalized> builder)
        {
            builder.ToTable("DocumentNormalized");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Content)
            .IsRequired();
        }
    }
}
