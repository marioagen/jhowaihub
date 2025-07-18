using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WoopiAiHub.Repository.Mappings
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .HasColumnName("Id")
                   .HasColumnType("uniqueidentifier")
                   .ValueGeneratedNever();

            builder.Property(u => u.Name)
                   .HasColumnName("Name")
                   .HasColumnType("varchar(150)")
                   .IsRequired();

            builder.Property(u => u.Email)
                   .HasColumnName("Email")
                   .HasColumnType("varchar(254)")
                   .IsRequired();

            builder.Property(u => u.IsActive)
                   .HasColumnName("IsActive")
                   .HasColumnType("bit")
                   .IsRequired();

            builder.Property(u => u.Created)
                   .HasColumnName("Created")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("PasswordHash")
                   .HasColumnType("varbinary(64)")
                   .IsRequired();

            builder.Property(u => u.Salt)
                   .HasColumnName("Salt")
                   .HasColumnType("varbinary(16)")
                   .IsRequired();

            builder.HasMany(u => u.Teams)
                   .WithMany(t => t.Users)
                   .UsingEntity<Dictionary<string, object>>(
                    "UserTeams",
                    r => r.HasOne<Team>().WithMany().HasForeignKey("TeamId"),
                    l => l.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    je =>
                    {
                        je.HasKey("UserId", "TeamId");
                        je.ToTable("UserTeams");
                        je.Property<Guid>("UserId").HasColumnName("UserId");
                        je.Property<int>("TeamId").HasColumnName("TeamId");
                    });

            builder.HasMany(u => u.Permissions)
                .WithMany(t => t.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserPermissions",
                    r => r.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                    l => l.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    je =>
                    {
                        je.HasKey("UserId", "PermissionId");
                        je.ToTable("UserPermissions");
                        je.Property<Guid>("UserId").HasColumnName("UserId");
                        je.Property<int>("PermissionId").HasColumnName("PermissionId");
                    });

            builder.HasMany(u => u.Profiles)
                .WithMany(t => t.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserProfiles",
                    r => r.HasOne<Profile>().WithMany().HasForeignKey("ProfileId"),
                    l => l.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    je =>
                    {
                        je.HasKey("UserId", "ProfileId");
                        je.ToTable("UserProfiles");
                        je.Property<Guid>("UserId").HasColumnName("UserId");
                        je.Property<int>("ProfileId").HasColumnName("ProfileId"); 
                    });
        }
    }
}
