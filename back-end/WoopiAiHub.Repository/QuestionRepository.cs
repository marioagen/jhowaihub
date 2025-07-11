using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace WoopiAiHub.Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly Context.ApplicationDbContext _context;

        public QuestionRepository(Context.ApplicationDbContext context)
        {
            _context = context;
        }

        public bool CreateUniqueQuestion(Question question)
        {

            var existQuestion = _context.Questions.Any(p => p.Description == question.Description);
            if(!existQuestion)
            {
                _context.Questions.Add(question);
                _context.SaveChanges();

                return true;
            }
            return false;

        }

        /// <summary>
        /// Find all questions and convert to a Dto list
        /// </summary>
        /// <returns></returns>
        public ICollection<QuestionDto> FindAll()
        {
            return _context.Questions
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Questionnaires = q.QuestionQuestionnaire.Select(qq => new Questionnaire
                    (
                      qq.Questionnaire.Title,
                      qq.Questionnaire.TypeDocId,
                      qq.Questionnaire.EmailCreator,
                      qq.Questionnaire.Id,
                      qq.Questionnaire.Created
                    )).ToList(),
                    Description = q.Description,
                    EmailCreator = q.EmailCreator,
                    Created = q.Created
                }).ToList();

        }

        /// <summary>
        /// Find a question by the description and convert to a Dto
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public QuestionDto FindByDescriptionAndEmail(string desc,
                                                     string email)
        {
            return _context.Questions.Where(a => a.Description.Equals(desc) && a.EmailCreator.Equals(email))
                                     .Select(q => new QuestionDto
                                     {
                                         Id = q.Id,
                                         Questionnaires = q.QuestionQuestionnaire.Select(qq => new Questionnaire
                                       (
                                         qq.Questionnaire.Title,
                                         qq.Questionnaire.TypeDocId,
                                         qq.Questionnaire.EmailCreator,
                                         qq.Questionnaire.Id,
                                         qq.Questionnaire.Created
                                       )).ToList(),
                                         Description = q.Description,
                                         EmailCreator = q.EmailCreator,
                                         Created = q.Created
                                     }).FirstOrDefault();
        }

        /// <summary>
        /// Find question by id and convert to a Dto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public QuestionDto FindById(int id)
        {
            return _context.Questions.Where(u => u.Id.Equals(id))
                                     .Select(q => new QuestionDto
                                     {
                                         Id = q.Id,
                                         Questionnaires = q.QuestionQuestionnaire.Select(qq => new Questionnaire
                                         (
                                           qq.Questionnaire.Title,
                                           qq.Questionnaire.TypeDocId,
                                           qq.Questionnaire.EmailCreator,
                                           qq.Questionnaire.Id,
                                           qq.Questionnaire.Created
                                         )).ToList(),
                                         Description = q.Description,
                                         EmailCreator = q.EmailCreator,
                                         Created = q.Created
                                     }).FirstOrDefault();
        }

        /// <summary>
        /// Find questions by ids and convert to a Dto
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Question> FindByIds(List<int> ids)
        {
            return _context.Questions.Where(u => ids.Contains(u.Id))
                                     .Include(u => u.QuestionQuestionnaire)
                                     .AsNoTracking()
                                     .ToList();

        }

        /// <summary>
        /// Delete questions
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            var questions = _context.Questions.Where(a => ids.Contains(a.Id));

            if (questions.Count() > 0)
            {
                _context.Questions.RemoveRange(questions);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Update a question
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public bool Update(QuestionUpdateDto updateQuestionDto)
        {
            var existQuestion = _context.Questions.Any(p => p.Description == updateQuestionDto.Description && p.Id != updateQuestionDto.Id);
            
            if (!existQuestion)
            {
                _context.Questions.Where(a => a.Id.Equals(updateQuestionDto.Id))
                                  .ExecuteUpdate(b => b
                                  .SetProperty(u => u.Description, updateQuestionDto.Description));

                _context.SaveChanges();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all questions paged
        /// </summary>
        /// <param name="questionPagedDataDto"></param>
        /// <returns></returns>
        public IQueryable<QuestionDto> FindAllPaged(QuestionPagedDataDto questionPagedDataDto)
        {
            var query = _context.Questions
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Questionnaires = q.QuestionQuestionnaire.Select(qq => new Questionnaire
                (
                  qq.Questionnaire.Title,
                  qq.Questionnaire.TypeDocId,
                  qq.Questionnaire.EmailCreator,
                  qq.Questionnaire.Id,
                  qq.Questionnaire.Created
                )).ToList(),
                Description = q.Description,
                EmailCreator = q.EmailCreator,
                Created = q.Created
            })
             .AsQueryable()
             .AsNoTracking();

            return query;
        }
    }
}
