using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class StepToolParameterRepository : IStepToolParameterRepository
    {
        private readonly ApplicationDbContext _context;

        public StepToolParameterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Delete StepToolParameters by their IDs.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIds(ICollection<int> ids)
        {
            var parameters = _context.StepToolParameters.Where(a => ids.Contains(a.Id));

            if (parameters.Any())
            {
                _context.StepToolParameters.RemoveRange(parameters);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Delete by step tool ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByStepToolsIds(ICollection<int> ids)
        {
            var parameters = _context.StepToolParameters.Where(a => ids.Contains(a.StepToolId));

            if (parameters.Any())
            {
                _context.StepToolParameters.RemoveRange(parameters);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        public string FindByStepToolId(int stepToolId)
        {
            var input = _context.StepToolParameters.Where(u => u.StepToolId.Equals(stepToolId) && u.StepTool.Tool.IsEditableInput)
                                                   .Select(v => v.Value)
                                                   .FirstOrDefault();

            return input;
        }
    }
}
