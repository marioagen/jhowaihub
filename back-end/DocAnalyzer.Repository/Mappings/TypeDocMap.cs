using DocAnalyzer.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAnalyzer.Repository.Mappings
{
    public class TypeDocMap
    {
        public void Configure(EntityTypeBuilder<TypeDoc> builder)
        {
            builder.ToTable("TypeDoc");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                   .IsRequired();

            builder.Property(u => u.EmailCreator)
                   .IsRequired();

            builder.HasMany(u => u.Questionnaires)
                   .WithOne(u => u.TypeDoc)
                   .HasForeignKey(u => u.TypeDocId);
        }
    }
}
