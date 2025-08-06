using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StatusMap : IEntityTypeConfiguration<Status>
    {
        public void Configure(EntityTypeBuilder<Status> builder)
        {
            builder.ToTable("Status");

            builder.HasKey(ss => ss.Id);

            builder.Property(ss => ss.Name)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(ss => ss.Created)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasMany(c => c.Cards)
                .WithOne(c => c.Status)
                .HasForeignKey(c => c.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Steps)
                .WithOne(c => c.Status)
                .HasForeignKey(c => c.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ss => ss.Name).IsUnique();
            builder.HasIndex(ss => ss.Created);
        }
    }
}
