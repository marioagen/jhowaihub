using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class UsageLogMap : IEntityTypeConfiguration<UsageLog>
    {
        public void Configure(EntityTypeBuilder<UsageLog> builder)
        {

            builder.HasKey(ul => ul.Id);

            builder.Property(ul => ul.NameTenant)
                   .HasColumnName("NameTenant")
                   .HasColumnType("varchar(150)")
                   .IsRequired();

            builder.Property(ul => ul.PeriodStart)
                   .HasColumnName("PeriodStart")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.Property(ul => ul.PeriodEnd)
                   .HasColumnName("PeriodEnd")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.Property(ul => ul.TotalUsage)
                   .HasColumnName("TotalUsage")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(ul => ul.NameType)
                   .HasColumnName("NameType")
                   .HasColumnType("varchar(100)")
                   .IsRequired();

            builder.Property(ul => ul.Created)
                   .HasColumnName("Created")
                   .HasColumnType("datetime")
                   .IsRequired();
        }
    }
}
