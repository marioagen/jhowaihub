using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class TenantMap : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("Tenants");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasColumnType("varchar(255)")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(c => c.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(e => e.DateStartSubscription)
               .HasColumnType("datetime")
               .IsRequired(false);

            builder.Property(e => e.DateEndSubscription)
               .HasColumnType("datetime")
               .IsRequired(false);

            builder.Property(e => e.DateRenewSubscription)
               .HasColumnType("datetime")
               .IsRequired(false);

            builder.Property(e => e.KeyAiGateway)
                .HasColumnType("varchar(255)")
                .HasMaxLength(255);

            builder.Property(w => w.IsActive)
               .HasColumnName("IsActive")
               .HasColumnType("bit")
               .IsRequired();

            builder.Property(c => c.PlanName)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(c => c.Name);
        }
    }
}
