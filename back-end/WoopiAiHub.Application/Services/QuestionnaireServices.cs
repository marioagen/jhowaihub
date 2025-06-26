using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Interfaces.Services;
using WoopiAiHub.Domain.Interfaces.Utils;
using WoopiAiHub.Domain.Models;
using System.Linq.Dynamic.Core;

namespace WoopiAiHub.Application.Services
{
    public class QuestionnaireServices : IQuestionnaireServices
    {
        public IQuestionnaireRepository _questionnaireRepository;
        public IQuestionServices _questionServices;
        public IQuestionQuestionnaireRepository _questionQuestionnaireRepository;
        private readonly IUnitOfWork _unitOfWork;

        public QuestionnaireServices(IQuestionnaireRepository questionnaireRepository,
                                     IQuestionServices questionServices,
                                     IQuestionQuestionnaireRepository questionQuestionnaireRepository,
                                     IUnitOfWork unitOfWork)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._questionServices = questionServices;
            this._questionQuestionnaireRepository = questionQuestionnaireRepository;
            this._unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Create a questionnaire by Dto
        /// </summary>
        /// <param name="sendQuestionnaireDto"></param>
        /// <returns></returns>
        public bool CreateUniqueQuestionnaire(CreateQuestionnaireDto createQuestionnaireDto,
                                              string email)
        {

            var questionnaire = this.GenerateQuestionnaireToCreate(createQuestionnaireDto,
                                                                   email);

            var questionnaireResult =  _questionnaireRepository.CreateUniqueQuestionnaire(questionnaire);

            if (!questionnaireResult)
            {
                throw new ArgumentException("Duplicated questionnaires");
            }

            return questionnaireResult;
        }

        /// <summary>
        /// Find all questionnaires
        /// </summary>
        /// <returns></returns>
        public ICollection<QuestionnaireDto> FindAll()
        {
            return _questionnaireRepository.FindAll();
        }

        /// <summary>
        /// Find a questionnaire by title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public QuestionnaireDto FindById(int id)
        {
            return _questionnaireRepository.FindById(id);
        }

        /// <summary>
        /// Delete questionnaires by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            return _questionnaireRepository.DeleteByIds(ids);
        }

        /// <summary>
        /// Update a questionnaire by Dto
        /// </summary>
        /// <param name="updateQuestionnaireDto"></param>
        /// <returns></returns>
        public bool Update(UpdateQuestionnaireDto updateQuestionnaireDto)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var questionnaireIdResult = _questionnaireRepository.FindById(updateQuestionnaireDto.Id);
                var questionnaire = GenerateQuestionnaireToUpdate(updateQuestionnaireDto, questionnaireIdResult);
                bool questionsDeleted = DeleteQuestions(updateQuestionnaireDto, questionnaireIdResult);

                if (!questionsDeleted)
                {
                    throw new ArgumentException("Failed to delete questions");
                    
                }

                bool questionnaireUpdated = _questionnaireRepository.Update(questionnaire);
                if (!questionnaireUpdated)
                {
                    throw new ArgumentException("Duplicated questionnaires");
                }

                _unitOfWork.Commit(); 
                return true;
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }

        }

        /// <summary>
        ///  Method for generate a questionnaire model to update method
        /// </summary>
        /// <param name="updateQuestionnaireDto"></param>
        /// <param name="questionnaireDto"></param>
        /// <returns></returns>
        private Questionnaire GenerateQuestionnaireToUpdate(UpdateQuestionnaireDto updateQuestionnaireDto,
                                                            QuestionnaireDto questionnaireDto)
        {
            Questionnaire questionnaire = new Questionnaire
           (updateQuestionnaireDto.Title,
            updateQuestionnaireDto.TypeDocId,
            questionnaireDto.EmailCreator,
            questionnaireDto.Id,
            questionnaireDto.Created);

            foreach (var questionId in updateQuestionnaireDto.QuestionsId)
            {
                var result = questionnaireDto.Questions.Any(x => x.Id.Equals(questionId));

                if (result is false)
                {
                    var questionDto = _questionServices.FindById(questionId);
                    var question =
                         new Question
                        (
                          questionDto.Description,
                          questionDto.EmailCreator,
                          questionDto.Id,
                          questionDto.Created
                         );

                    questionnaire.QuestionQuestionnaire.Add(new QuestionQuestionnaire(question, questionnaire));
                }
            }
            return questionnaire;
        }

        /// <summary>
        /// Method for create questionnaire model to create method
        /// </summary>
        /// <param name="createQuestionnaireDto"></param>
        /// <returns></returns>
        private Questionnaire GenerateQuestionnaireToCreate(CreateQuestionnaireDto createQuestionnaireDto,
                                                            string email)
        {
            Questionnaire questionnaire = new Questionnaire
               (createQuestionnaireDto.Title,
                createQuestionnaireDto.TypeDocId,
                email,
                0,
                DateTime.Now);

            foreach (var questionId in createQuestionnaireDto.QuestionsId)
            {
                var questionDto = _questionServices.FindById(questionId);
                var question =
                     new Question
                    (
                      questionDto.Description,
                      questionDto.EmailCreator,
                      questionDto.Id,
                      questionDto.Created
                     );

                questionnaire.QuestionQuestionnaire.Add(new QuestionQuestionnaire(question.Id, questionnaire.Id));
            }
            return questionnaire;
        }

        /// <summary>
        /// Method used find and delete questions
        /// </summary>
        /// <param name="updateQuestionnaireDto"></param>
        /// <param name="questionnaireDto"></param>
        /// <returns></returns>
        private bool DeleteQuestions(UpdateQuestionnaireDto updateQuestionnaireDto,
                                     QuestionnaireDto questionnaireDto)
        {
            var deleteQuestions = questionnaireDto.Questions.Where(x => !updateQuestionnaireDto.QuestionsId.Contains(x.Id)).ToList();
            if (deleteQuestions.Count > 0)
            {
                return _questionQuestionnaireRepository.Delete(deleteQuestions);
            }

            return true;
        }

        /// <summary>
        /// Get all questionnaires paged
        /// </summary>
        /// <param name="updatequestionDto"></param>
        /// <returns></returns>
        public QuestionnairePagedResultDto FindAllPaged(QuestionnairePagedDataDto questionnairePagedDataDto)
        {

            if (questionnairePagedDataDto.Page > 0)
            {
                var totalList = _questionnaireRepository.FindAllPaged(questionnairePagedDataDto);

                if (questionnairePagedDataDto.ColType != ColTypeQuestionnaire.TypeDoc)
                {
                    totalList = questionnairePagedDataDto.IsAscending ?
                    totalList.OrderBy(questionnairePagedDataDto.ColType.ToString()) :
                    totalList.OrderBy(questionnairePagedDataDto.ColType.ToString() + " descending");
                }
                else
                {
                    totalList = questionnairePagedDataDto.IsAscending ?
                    totalList.OrderBy(u => u.TypeDocName) :
                    totalList = totalList.OrderByDescending(u => u.TypeDocName);

                }

                var result = this.QuestionnairesPagination(totalList, questionnairePagedDataDto);
                return result;
            }
            else
            {
                var ex = new ArgumentException("The number of pages must be greater than 0");
                throw ex;
            }
        }

        /// <summary>
        /// Ordenates the list of questionnaires and returns a paged result
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="questionnairePagedDataDto"></param>
        /// <returns></returns>
        private QuestionnairePagedResultDto QuestionnairesPagination(IQueryable<QuestionnaireDto> totalList,
                                                                     QuestionnairePagedDataDto questionnairePagedDataDto)
        {

            int pageCount, currentPage = 0;

            if (string.IsNullOrEmpty(questionnairePagedDataDto.Search) is false)
            {
                string search = questionnairePagedDataDto.Search.ToLower();
                totalList = totalList.Where(i => i.Title.ToLower().Contains(search)
                || i.Id.ToString().Contains(search)
                || i.TypeDocName.ToLower().Contains(search));
            }

            var totalListCount = totalList.Count();

            if (questionnairePagedDataDto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
                questionnairePagedDataDto.PageSize = totalListCount;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)totalListCount / questionnairePagedDataDto.PageSize);
                currentPage = questionnairePagedDataDto.Page <= pageCount ? questionnairePagedDataDto.Page : 1;
                totalList = totalList.Skip((currentPage - 1) * questionnairePagedDataDto.PageSize)
                                     .Take(questionnairePagedDataDto.PageSize);
            }

            return new QuestionnairePagedResultDto()
            {
                Content = totalList,
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalListCount
            };
        }
    }
}
