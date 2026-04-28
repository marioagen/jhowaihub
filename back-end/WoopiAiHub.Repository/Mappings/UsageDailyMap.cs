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
                   .HasColumnType("int");

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

            builder.HasOne(ud => ud.Workflow)
                   .WithMany(me => me.UsageDailies)
                   .HasForeignKey(ud => ud.WorkflowId);

            builder.HasIndex(ud => new { ud.Processed, ud.Created })
                   .HasDatabaseName("IX_UsageDaily_Processed_Created");

            builder.HasIndex(ud => ud.Created)
                   .HasDatabaseName("IX_UsageDaily_Created");

            builder.HasIndex(ud => new { ud.WorkflowId, ud.Processed })
                   .HasDatabaseName("IX_UsageDaily_WorkflowId_Processed");

            builder.HasIndex(ud => new { ud.UserId, ud.Processed })
                   .HasDatabaseName("IX_UsageDaily_UserId_Processed");
        }
    }
}
