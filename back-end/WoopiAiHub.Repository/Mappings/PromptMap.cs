using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class PromptMap : IEntityTypeConfiguration<Prompt>
    {
        public void Configure(EntityTypeBuilder<Prompt> builder)
        {
            builder.ToTable("Prompts");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.Description)
                   .IsRequired()
                   .HasMaxLength(95);

            builder.Property(u => u.Text)
                   .IsRequired()
                   .HasColumnType("nvarchar(max)");

            builder.Property(u => u.Created)
                   .IsRequired();

            builder.Property(u => u.IsEdited)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(u => u.IsImported)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne(d => d.User)
                   .WithMany(d => d.Prompts)
                   .HasForeignKey(d => d.IdUser)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
