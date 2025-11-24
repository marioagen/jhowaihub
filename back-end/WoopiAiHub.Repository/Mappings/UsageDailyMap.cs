using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class UsageDailyMap : IEntityTypeConfiguration<UsageDaily>
    {
        public void Configure(EntityTypeBuilder<UsageDaily> builder)
        {
            builder.HasKey(ud => ud.Id);

            builder.Property(ud => ud.UserId)
                   .HasColumnName("UserId")
                   .HasColumnType("uniqueidentifier")
                   .IsRequired();

            builder.Property(ud => ud.UsageTypeId)
                   .HasColumnName("UsageTypeId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(ud => ud.UsageCount)
                   .HasColumnName("UsageCount")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(ud => ud.Processed)
                   .HasColumnName("Processed")
                   .HasColumnType("bit")
                   .IsRequired();

            builder.Property(ud => ud.ModelEmbeddingId)
                   .HasColumnName("ModelEmbeddingId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(ud => ud.Created)
                   .HasColumnName("Created")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.HasOne(ud => ud.UsageType)
                   .WithMany(ut => ut.UsageDaily)
                   .HasForeignKey(ud => ud.UsageTypeId);

            builder.HasOne(ud => ud.ModelEmbedding)
                   .WithMany(me => me.UsageDaily)
                   .HasForeignKey(ud => ud.ModelEmbeddingId);

            builder.HasOne(ud => ud.User)
                   .WithMany(me => me.UsageDailies)
                   .HasForeignKey(ud => ud.UserId);
        }
    }
}
