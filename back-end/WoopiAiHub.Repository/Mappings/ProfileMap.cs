using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class ProfileMap : IEntityTypeConfiguration<Profile>
    {
        private const string PermissionIdColumn = "PermissionId";
        private const string ProfileIdColumn = "ProfileId";

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
                       r => r.HasOne<Permission>()
                            .WithMany()
                            .HasForeignKey(PermissionIdColumn)
                            .OnDelete(DeleteBehavior.Restrict),
                       l => l.HasOne<Profile>()
                            .WithMany()
                            .HasForeignKey(ProfileIdColumn)
                            .OnDelete(DeleteBehavior.Cascade),
                       je =>
                       {
                           je.HasKey(PermissionIdColumn, ProfileIdColumn);
                           je.ToTable("ProfilePermissions");
                           je.Property<int>(ProfileIdColumn).HasColumnName(ProfileIdColumn);
                           je.Property<int>(PermissionIdColumn).HasColumnName(PermissionIdColumn);
                       });
                    }
    }
}
