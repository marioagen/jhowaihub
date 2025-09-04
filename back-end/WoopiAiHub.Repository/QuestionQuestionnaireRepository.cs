using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;


namespace WoopiAiHub.Repository
{
    public class QuestionQuestionnaireRepository : IQuestionQuestionnaireRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public QuestionQuestionnaireRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Delete a question from a questionnaire and the relationship between them
        /// </summary>
        /// <param name="questions"></param>
        /// <returns></returns>
        public bool Delete(ICollection<Question> questions)
        {
            var idsQuestionToDelete = questions.Select(q => q.Id).ToList();

            var questionsToDelete = _context.QuestionQuestionnaire
                                              .Where(q => idsQuestionToDelete.Contains(q.QuestionId))
                                              .ToList();

            _context.QuestionQuestionnaire.RemoveRange(questionsToDelete);
            _context.SaveChanges();
            return true;
        }
    }
}
