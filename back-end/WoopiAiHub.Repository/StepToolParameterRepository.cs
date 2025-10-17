using Microsoft.EntityFrameworkCore;
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
        public bool DeleteByIds(IEnumerable<int> ids)
        {
            if (!ids?.Any() ?? true)
                return false;

            var deletedCount = _context.StepToolParameters
                .Where(a => ids!.Contains(a.Id))
                .ExecuteDelete();

            return deletedCount > 0;
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

        /// <summary>
        /// Retrieves the value associated with the specified step tool identifier.
        /// </summary>
        /// <remarks>This method searches for a step tool with the given identifier where the associated
        /// tool  is marked as editable input. If multiple matches exist, the first value is returned.</remarks>
        /// <param name="stepToolId">The identifier of the step tool to search for.</param>
        /// <returns>The value associated with the specified step tool identifier, or <see langword="null"/>  if no matching step
        /// tool is found.</returns>
        public StepToolParameter? FindByStepToolId(int stepToolId)
        {
            var input = _context.StepToolParameters.Where(u => u.StepToolId.Equals(stepToolId) && u.StepTool!.Tool!.IsEditableInput)
                                                   .FirstOrDefault();

            return input;
        }
    }
}
