using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class SubscriptionPeriodMap : IEntityTypeConfiguration<SubscriptionPeriod>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPeriod> builder)
        {
            builder.ToTable("SubscriptionPeriods");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.PeriodStart)
                   .IsRequired();

            builder.Property(u => u.PeriodEnd)
                   .IsRequired();

            builder.Property(u => u.IsProcessed)
                   .IsRequired();

            builder.Property(u => u.Created)
                   .IsRequired();
        }
    }
}
