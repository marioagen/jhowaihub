using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class ApiTemplateMap : IEntityTypeConfiguration<ApiTemplate>
    {
        public void Configure(EntityTypeBuilder<ApiTemplate> builder)
        {
            builder.ToTable("ApiTemplates");

            builder.HasKey(p => p.Id);

            builder.Property(k => k.Id)
                   .ValueGeneratedOnAdd()
                   .IsRequired();

            builder.Property(k => k.Created)
                   .ValueGeneratedOnAdd()
                   .IsRequired()
                   .HasDefaultValueSql("(GETDATE())");
        }
    }
}
