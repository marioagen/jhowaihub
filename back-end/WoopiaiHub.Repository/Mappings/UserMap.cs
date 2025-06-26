using DocAnalyzer.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocAnalyzer.Repository.Mappings
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

            builder.HasMany(u => u.Teams)
                   .WithMany(t => t.Users)
                   .UsingEntity<Dictionary<string, object>>(
                    "UserTeam",
                    r => r.HasOne<Team>().WithMany().HasForeignKey("TeamId"),
                    l => l.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    je =>
                    {
                        je.HasKey("UserId", "TeamId");
                        je.ToTable("UserTeam");
                        je.Property<Guid>("UserId").HasColumnName("UserId");
                        je.Property<int>("TeamId").HasColumnName("TeamId");
                    });
        }
    }
}
