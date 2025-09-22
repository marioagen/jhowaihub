using WoopiAiHub.Domain.DTOs;
using WoopiAiHub.Domain.DTOs.Request;
using WoopiAiHub.Domain.DTOs.Response;
using WoopiAiHub.Domain.Interfaces.Services;

namespace WoopiAiHub.Application.Services
{
    public class AutomationServices : IAutomationServices
    {
        public AutomationServices()
        {
            /// <summary>
            /// Find all questions
            /// </summary>
            /// <returns></returns>
        public ICollection<AutomationDto> FindAll()
        {
            return _questionRepository.FindAll();
        }

        /// <summary>
        /// Find a question by description
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public AutomationDto FindByDescriptionAndEmail(string desc,
                                                     string emailCreator)
        {
            return _questionRepository.FindByDescriptionAndEmail(desc,
                                                                 emailCreator);
        }

        /// <summary>
        /// Find a question by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public QuestionDto FindById(int id)
        {
            return _questionRepository.FindById(id);
        }

        /// <summary>
        /// Delete questions by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(List<int> ids)
        {
            var idsQuestionnaires = _questionnaireRepository.FindByQuestionIds(ids);
            var result = _questionRepository.DeleteByIds(ids);

            if (result)
            {
                var questionnaireList = _questionnaireRepository.FindByIds(idsQuestionnaires);

                foreach (var q in questionnaireList)
                {
                    var emptyQuestions = q.QuestionQuestionnaire.Count == 0;
                    if (emptyQuestions)
                    {
                        _questionnaireRepository.DeleteById(q.Id);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Update question by dto
        /// </summary>
        /// <param name="updatequestionDto"></param>
        /// <returns></returns>
        public bool Update(QuestionUpdateDto updatequestionDto)
        {
            var questionResult = _questionRepository.Update(updatequestionDto);
            if (!questionResult)
            {
                throw new ArgumentException("Duplicated TypeDoc");

            }

            return questionResult;
        }

        /// <summary>
        /// Get all questions paged
        /// </summary>
        /// <param name="updatequestionDto"></param>
        /// <returns></returns>
        public QuestionPagedResultDto FindAllPaged(QuestionPagedDataDto questionPagedDataDto)
        {
            if (questionPagedDataDto.Page > 0)
            {
                var totalList = _questionRepository.FindAllPaged(questionPagedDataDto);

                totalList = questionPagedDataDto.IsAscending ?
                totalList.OrderBy(questionPagedDataDto.ColType.ToString()) :
                totalList.OrderBy(questionPagedDataDto.ColType.ToString() + " descending");

                var result = this.QuestionPagination(totalList, questionPagedDataDto);
                return result;
            }
            else
            {
                var ex = new ArgumentException("The number of pages must be greater than 0");
                throw ex;
            }
        }

        /// <summary>
        /// Ordenates the list of Questions and returns a paged result
        /// </summary>
        /// <param name="totalList"></param>
        /// <param name="questionPagedDataDto"></param>
        /// <returns></returns>
        private QuestionPagedResultDto QuestionPagination(IQueryable<QuestionDto> totalList,
                                                          QuestionPagedDataDto questionPagedDataDto)
        {
            int pageCount, currentPage = 0;

            if (string.IsNullOrEmpty(questionPagedDataDto.Search) is false)
            {
                totalList = totalList.Where(i => i.Description.ToLower()
                                     .Contains(questionPagedDataDto.Search.ToLower()) ||
                                               i.Id.ToString().Contains(questionPagedDataDto.Search));
            }

            var totalListCount = totalList.Count();

            if (questionPagedDataDto.PageSize == 0)
            {
                pageCount = 1;
                currentPage = 1;
                questionPagedDataDto.PageSize = totalListCount;
            }
            else
            {
                pageCount = (int)Math.Ceiling((double)totalListCount / questionPagedDataDto.PageSize);
                currentPage = questionPagedDataDto.Page <= pageCount ? questionPagedDataDto.Page : 1;
                totalList = totalList.Skip((currentPage - 1) * questionPagedDataDto.PageSize)
                                     .Take(questionPagedDataDto.PageSize);
            }

            return new QuestionPagedResultDto()
            {
                Content = totalList,
                CurrentPage = currentPage,
                PageCount = pageCount,
                RowCount = totalListCount,
            };
        }
    }
}
        } 
    }
}
