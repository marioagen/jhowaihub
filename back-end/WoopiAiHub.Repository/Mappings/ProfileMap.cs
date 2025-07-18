using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class ProfileMap
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.ToTable("Profiles");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                   .IsRequired();

            builder.Property(u => u.Created)
                   .IsRequired();

            builder.HasMany(p => p.Permissions)
                   .WithMany(pr => pr.Profiles)
                   .UsingEntity<Dictionary<string, object>>(
                       "ProfilePermissions",
                       r => r.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                       l => l.HasOne<Profile>().WithMany().HasForeignKey("ProfileId"),
                       je =>
                       {
                           je.HasKey("PermissionId", "ProfileId");
                           je.ToTable("ProfilePermissions");
                           je.Property<int>("ProfileId").HasColumnName("ProfileId");
                           je.Property<int>("PermissionId").HasColumnName("PermissionId");
                       });
                    }
    }
}
