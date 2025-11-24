using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class ModelEmbeddingsMap : IEntityTypeConfiguration<ModelEmbedding>
    {
        public void Configure(EntityTypeBuilder<ModelEmbedding> builder)
        {
            builder.HasKey(me => me.Id);

            builder.Property(me => me.Name)
                   .HasColumnName("Name")
                   .HasColumnType("varchar(150)")
                   .IsRequired();

            builder.Property(me => me.Created)
                   .HasColumnName("Created")
                   .HasColumnType("datetime")
                   .IsRequired();

            builder.HasMany(me => me.UsageDaily)
                   .WithOne(ud => ud.ModelEmbedding)
                   .HasForeignKey(ud => ud.ModelEmbeddingId);

            builder.HasMany(me => me.UsageMonth)
                   .WithOne(um => um.ModelEmbedding)
                   .HasForeignKey(um => um.ModelEmbeddingId);
        }
    }
}
