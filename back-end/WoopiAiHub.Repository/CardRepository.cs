using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Interfaces.Repository;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Context;

namespace WoopiAiHub.Repository
{
    public class CardRepository : ICardRepository
    {
        private readonly ApplicationDbContext _context;
        public CardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// checks if a collection of card IDs exists in the database.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> ExistsStepsInUse(ICollection<int> ids)
        {
           return await _context.Cards.Where(a => ids.Contains(a.StepId)).AnyAsync();
        }

        /// <summary>
        /// Returns a card by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Card?> FindById(int id)
        {
            return await _context.Cards
                                 .Where(c => c.Id == id)
                                 .Include(d => d.Document)
                                 .Include(s => s.Step)
                                 .ThenInclude(w => w!.Workflow)
                                 .ThenInclude(t => t!.Team)
                                 .ThenInclude(u => u!.Users)
                                 .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates a card.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool Update(Card card)
        {
            _context.Cards.Update(card);
            return _context.SaveChanges() > 0;
        }

        /// <summary>
        /// Deletes a card by its document id.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public async Task<bool> DeleteByDocumentIds(List<int> documentIds)
        {
            var cards = _context.Cards.Where(c => documentIds.Contains(c.DocumentId));

            if (await cards.AnyAsync())
            {
                await cards.ExecuteUpdateAsync(b => b
                           .SetProperty(u => u.Enable, false));

                return await _context.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
