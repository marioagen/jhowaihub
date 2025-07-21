using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WoopiAiHub.Repository.Mappings
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        private const string UserIdColumn = "UserId";
        private const string TeamIdColumn = "TeamId";
        private const string PermissionIdColumn = "PermissionId";
        private const string ProfileIdColumn = "ProfileId";

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
                    r => r.HasOne<Team>().WithMany().HasForeignKey(TeamIdColumn),
                    l => l.HasOne<User>().WithMany().HasForeignKey(UserIdColumn),
                    je =>
                    {
                        je.HasKey(UserIdColumn, TeamIdColumn);
                        je.ToTable("UserTeams");
                        je.Property<Guid>(UserIdColumn).HasColumnName(UserIdColumn);
                        je.Property<int>(TeamIdColumn).HasColumnName(TeamIdColumn);
                    });

            builder.HasMany(u => u.Permissions)
                .WithMany(t => t.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserPermissions",
                    r => r.HasOne<Permission>().WithMany().HasForeignKey(PermissionIdColumn),
                    l => l.HasOne<User>().WithMany().HasForeignKey(UserIdColumn),
                    je =>
                    {
                        je.HasKey(UserIdColumn, PermissionIdColumn);
                        je.ToTable("UserPermissions");
                        je.Property<Guid>(UserIdColumn).HasColumnName(UserIdColumn);
                        je.Property<int>(PermissionIdColumn).HasColumnName(PermissionIdColumn);
                    });

            builder.HasMany(u => u.Profiles)
                .WithMany(t => t.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserProfiles",
                    r => r.HasOne<Profile>().WithMany().HasForeignKey(ProfileIdColumn),
                    l => l.HasOne<User>().WithMany().HasForeignKey(UserIdColumn),
                    je =>
                    {
                        je.HasKey(UserIdColumn, ProfileIdColumn);
                        je.ToTable("UserProfiles");
                        je.Property<Guid>(UserIdColumn).HasColumnName(UserIdColumn);
                        je.Property<int>(ProfileIdColumn).HasColumnName(ProfileIdColumn); 
                    });
        }
    }
}
