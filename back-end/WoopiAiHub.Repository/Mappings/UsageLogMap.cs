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

            builder.Property(ul => ul.UserId)
                   .HasColumnName("UserId")
                   .HasColumnType("uniqueidentifier")
                   .IsRequired();

            builder.Property(ul => ul.UsageTypeId)
                   .HasColumnName("UsageTypeId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(ul => ul.UsageCount)
                   .HasColumnName("UsageCount")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(ul => ul.Processed)
                   .HasColumnName("Processed")
                   .HasColumnType("bit")
                   .IsRequired();

            builder.Property(ul => ul.ModelEmbeddingId)
                   .HasColumnName("ModelEmbeddingId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(ul => ul.Created)
                   .HasColumnName("Created")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.HasOne(ul => ul.UsageType)
                   .WithMany(ut => ut.UsageLog)
                   .HasForeignKey(ul => ul.UsageTypeId);

            builder.HasOne(ul => ul.ModelEmbedding)
                   .WithMany(me => me.UsageLog)
                   .HasForeignKey(ul => ul.ModelEmbeddingId);

            builder.HasOne(ul => ul.User)
                   .WithMany(u => u.UsageLogs)
                   .HasForeignKey(ul => ul.UserId);
        }
    }
}
