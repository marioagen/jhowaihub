using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Mappings
{
    public class StepProfilePermissionMap : IEntityTypeConfiguration<StepProfilePermission>
    {
        public void Configure(EntityTypeBuilder<StepProfilePermission> builder)
        {
            builder.ToTable("StepProfilePermissions");

            builder.HasKey(x => new { x.StepId, x.ProfileId, x.PermissionId });

            builder.HasOne(x => x.Step)
                .WithMany(s => s.StepProfilePermissions)
                .HasForeignKey(x => x.StepId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Profile)
                .WithMany(p => p.StepProfilePermissions)
                .HasForeignKey(x => x.ProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Permission)
                .WithMany(pr => pr.StepProfilePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}