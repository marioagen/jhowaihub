using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class PermissionMap : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                   .IsRequired();

            builder.Property(u => u.Description)
                  .IsRequired();

            builder.Property(u => u.Group)
                  .IsRequired();

            builder.Property(u => u.Created)
                   .IsRequired();
        }
    }
}
