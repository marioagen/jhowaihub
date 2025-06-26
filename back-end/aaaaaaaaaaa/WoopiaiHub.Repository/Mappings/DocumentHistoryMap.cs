using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Repository.Mappings
{
    public class DocumentHistoryMap : IEntityTypeConfiguration<DocumentHistory>
    {
        public void Configure(EntityTypeBuilder<DocumentHistory> builder)
        {
            builder.ToTable("DocumentHistories");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Input)
                   .IsRequired();

            builder.Property(u => u.Output)
                   .IsRequired();

            builder.Property(u => u.IsEdited)
                  .IsRequired();

            builder.HasOne(u => u.Document)
                   .WithMany(s => s.DocumentHistories)
                   .HasForeignKey(c => c.IdDocument);
        }
    }
}
