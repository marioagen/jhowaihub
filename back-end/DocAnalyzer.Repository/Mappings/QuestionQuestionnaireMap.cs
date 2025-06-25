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
    public class QuestionQuestionnaireMap : IEntityTypeConfiguration<QuestionQuestionnaire>
    {
        public void Configure(EntityTypeBuilder<QuestionQuestionnaire> builder)
        {
            builder.ToTable("QuestionQuestionnaire");

            builder.HasKey(qq => new { qq.QuestionId, qq.QuestionnaireId });

            builder.HasOne(qq => qq.Question)
                .WithMany(q => q.QuestionQuestionnaire)
                .HasForeignKey(qq => qq.QuestionId);

            builder.HasOne(qq => qq.Questionnaire)
                .WithMany(qn => qn.QuestionQuestionnaire)
                .HasForeignKey(qq => qq.QuestionnaireId);
        }
    }
}
