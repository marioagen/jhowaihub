using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class DocumentBatchMap : IEntityTypeConfiguration<DocumentBatch>
    {
        public void Configure(EntityTypeBuilder<DocumentBatch> builder)
        {
            builder.ToTable("DocumentBatchs");

            builder.HasKey(u => u.Id);
        }
    }
}
