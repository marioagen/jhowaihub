using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Repository.Mappings
{
    public class QuestionnaireMap : IEntityTypeConfiguration<Questionnaire>
    {
        public void Configure(EntityTypeBuilder<Questionnaire> builder)
        {
            builder.ToTable("Questionnaires");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Title)
                   .IsRequired();

            builder.Property(u => u.TypeDocId)
                   .IsRequired();

            builder.Property(u => u.EmailCreator)
                   .IsRequired();

            builder.HasOne(u => u.TypeDoc)
                   .WithMany(u => u.Questionnaires)
                   .HasForeignKey(u => u.TypeDocId);
        }
    }
}
