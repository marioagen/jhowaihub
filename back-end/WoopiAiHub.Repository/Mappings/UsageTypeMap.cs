using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class UsageTypeMap : IEntityTypeConfiguration<UsageType>
    {
        public void Configure(EntityTypeBuilder<UsageType> builder)
        {
            builder.HasKey(ut => ut.Id);

            builder.Property(ut => ut.Name)
                   .HasColumnName("Name")
                   .HasColumnType("varchar(100)")
                   .IsRequired();

            builder.Property(ut => ut.Created)
                   .HasColumnName("Created")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.HasMany(ut => ut.Units)
                   .WithOne(u => u.UsageType)
                   .HasForeignKey(u => u.UsageTypeId);

            builder.HasMany(ut => ut.UsageDaily)
                   .WithOne(ud => ud.UsageType)
                   .HasForeignKey(ud => ud.UsageTypeId);

            builder.HasMany(ut => ut.UsageMonth)
                   .WithOne(um => um.UsageType)
                   .HasForeignKey(um => um.UsageTypeId);
        }
    }
}
