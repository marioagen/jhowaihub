using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using WoopiAiHub.Domain.Utils;

namespace WoopiAiHub.Repository
{
    public class QuestionnaireRepository : IQuestionnaireRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public QuestionnaireRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a questionnaire in the database
        /// </summary>
        /// <param name="questionnaire"></param>
        /// <returns></returns>
        public bool CreateUniqueQuestionnaire(Questionnaire questionnaire)
        {
            var existQuestionnaire = _context.Questionnaires.Any(p => p.Title == questionnaire.Title);
            if (!existQuestionnaire)
            {
                _context.Questionnaires.Add(questionnaire);
                _context.SaveChanges();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Find all questionnaires and return a dto collection
        /// </summary>
        /// <returns></returns>
        public ICollection<QuestionnaireDto> FindAll()
        {
            return _context.Questionnaires
                 .Select(q => new QuestionnaireDto
                 {
                     Id = q.Id,
                     Questions = q.QuestionQuestionnaire.Select(qq => new Question
                     (
                          qq.Question.Description,
                          qq.Question.EmailCreator,
                          qq.Question.Id,
                          qq.Question.Created

                     )).ToList(),
                     TypeDoc = new TypeDoc
                     (
                         q.TypeDoc.Name,
                         q.TypeDoc.EmailCreator,
                         q.TypeDoc.Id,
                         q.TypeDoc.Created
                     ),
                     Title = q.Title,
                     Created = q.Created,
                     TypeDocId = q.TypeDocId,
                     EmailCreator = q.EmailCreator
                 }).ToList();

        }

        /// <summary>
        /// Find a questionnaire by the id and return a dto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public QuestionnaireDto FindById(int id)
        {
            return _context.Questionnaires.Where(a => a.Id.Equals(id))
                                          .Select(q => new QuestionnaireDto
                                          {
                                              Id = q.Id,
                                              Questions = q.QuestionQuestionnaire.Select(qq => new Question
                                          (
                                             qq.Question.Description,
                                             qq.Question.EmailCreator,
                                             qq.Question.Id,
                                             qq.Question.Created

                                         )).ToList(),
                                              TypeDoc = new TypeDoc
                                           (
                                             q.TypeDoc.Name,
                                             q.TypeDoc.EmailCreator,
                                             q.TypeDoc.Id,
                                             q.TypeDoc.Created
                                            ),
                                              Title = q.Title,
                                              Created = q.Created,
                                              EmailCreator = q.EmailCreator
                                          })
                                        .FirstOrDefault();
        }


        /// <summary>
        /// Find a questionnaire by the id and return questionnaires
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Questionnaire> FindByIds(List<int> ids)
        {
            return _context.Questionnaires.Where(u => ids.Contains(u.Id))
                                     .Include(u => u.QuestionQuestionnaire)
                                     .AsNoTracking()
                                     .ToList();
        }

        /// <summary>
        /// Find a id of questionnaire by the question id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<int> FindByQuestionIds(List<int> ids)
        {
            return _context.Questionnaires
                   .Where(a => a.QuestionQuestionnaire.Any(qq => ids.Contains(qq.QuestionId)))
                   .Select(u => u.Id)
                   .ToList();

        }

        /// <summary>
        /// Delete questionnaires by multiple ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            var questionnaires = _context.Questionnaires.Where(a => ids.Contains(a.Id));
            var validationQuestionnaireUsedInTools = VerifyIfQuestionnaireIsUsedInTheWorkflowTools(ids);

            if (questionnaires.Any() && !validationQuestionnaireUsedInTools)
            {
                _context.Questionnaires.RemoveRange(questionnaires);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private bool VerifyIfQuestionnaireIsUsedInTheWorkflowTools(List<int> ids)
        {
            var toolTypeId = _context.ToolTypes.Where(tt => tt.Name == HandlersTypes.Quiz).Select(tt => tt.Id).FirstOrDefault();
            var idsString = ids.Select(i => i.ToString());
            return  _context.StepTools
                .Include(st => st.Tool!)
                .ThenInclude(t => t!.ToolType)
                .Where(st => st.Tool!.ToolType!.Id == toolTypeId)
                .Where(st => st.Parameters.Select(s => s.Value).Any(s => idsString.Contains(s)))
                .Any();
        }

        /// <summary>
        /// Delete questionnaire by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteById(int id)
        {
            var questionnaire = _context.Questionnaires.Where(a => a.Id.Equals(id)).FirstOrDefault();

            if (questionnaire is not null)
            {
                _context.Questionnaires.Remove(questionnaire);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Update a questionnaire
        /// </summary>
        /// <param name="questionnaire"></param>
        /// <returns></returns>
        public bool Update(Questionnaire questionnaire)
        {
            var existQuestionnaire = _context.Questionnaires.Any(q => q.Title == questionnaire.Title && q.Id != questionnaire.Id);
            if (!existQuestionnaire)
            {
                _context.Questionnaires.Update(questionnaire);
                _context.SaveChanges();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all questionnaires paged
        /// </summary>
        /// <param name="questionnairePagedDataDto"></param>
        /// <returns></returns>
        public IQueryable<QuestionnaireDto> FindAllPaged(QuestionnairePagedDataDto questionnairePagedDataDto)
        {
            var query = _context.Questionnaires
                        .Select(q => new QuestionnaireDto
                        {
                            Id = q.Id,
                            Questions = q.QuestionQuestionnaire.Select(qq => new Question
                            (
                              qq.Question.Description,
                              qq.Question.EmailCreator,
                              qq.Question.Id,
                              qq.Question.Created

                            )).ToList(),
                            TypeDoc = new TypeDoc
                              (
                                 q.TypeDoc.Name,
                                 q.TypeDoc.EmailCreator,
                                 q.TypeDoc.Id,
                                 q.TypeDoc.Created
                              ),
                            TypeDocName = q.TypeDoc.Name,
                            Title = q.Title,
                            Created = q.Created,
                            TypeDocId = q.TypeDocId,
                            EmailCreator = q.EmailCreator
                        }).AsQueryable()
                          .AsNoTracking();
            return query;
        }

    }
}
