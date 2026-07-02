using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class TenantLlmModelSettingMap : IEntityTypeConfiguration<TenantLlmModelSetting>
    {
        public void Configure(EntityTypeBuilder<TenantLlmModelSetting> builder)
        {
            builder.ToTable("TenantLlmModelSettings");
            builder.HasKey(x => x.Scope);

            builder.Property(x => x.Scope)
                .HasColumnName("Scope")
                .HasColumnType("varchar(50)")
                .IsRequired();

            builder.Property(x => x.ModelName)
                .HasColumnName("ModelName")
                .HasColumnType("varchar(150)")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(x => x.UpdatedByEmail)
                .HasColumnName("UpdatedByEmail")
                .HasColumnType("varchar(255)")
                .IsRequired();
        }
    }
}
