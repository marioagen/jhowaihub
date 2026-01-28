using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class AuditLogMap : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.TableName)
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.UserName)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(c => c.Action)
                .HasColumnType("varchar(max)")
                .IsRequired();

            builder.HasOne(d => d.User)
                .WithMany(d => d.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .IsRequired();

            builder.HasIndex(c => c.TableName);
        }
    }
}
