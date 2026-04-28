using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class UsageMonthMap : IEntityTypeConfiguration<UsageMonth>
    {
        public void Configure(EntityTypeBuilder<UsageMonth> builder)
        {
            builder.HasKey(um => um.Id);

            builder.Property(um => um.UsageTypeId)
                   .HasColumnName("UsageTypeId")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(um => um.Total)
                   .HasColumnName("Total")
                   .HasColumnType("int")
                   .IsRequired();

            builder.Property(um => um.ModelEmbeddingId)
                   .HasColumnName("ModelEmbeddingId")
                   .HasColumnType("int");

            builder.Property(um => um.UserId)
                   .HasColumnName("UserId")
                   .HasColumnType("uniqueidentifier")
                   .IsRequired();

            builder.Property(um => um.Created)
                   .HasColumnName("Created")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.HasOne(um => um.UsageType)
                   .WithMany(ut => ut.UsageMonth)
                   .HasForeignKey(um => um.UsageTypeId);

            builder.HasOne(um => um.ModelEmbedding)
                   .WithMany(me => me.UsageMonth)
                   .HasForeignKey(um => um.ModelEmbeddingId);

            builder.HasOne(um => um.User)
                   .WithMany(u => u.UsageMonths)
                   .HasForeignKey(um => um.UserId);

            builder.HasOne(um => um.Workflow)
                   .WithMany(u => u.UsageMonths)
                   .HasForeignKey(um => um.WorkflowId);
        }
    }
}
