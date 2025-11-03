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
                                    .ThenInclude(p => p!.Profile)
                                 .Include(s => s.Step)
                                 .ThenInclude(w => w!.Workflow)
                                 .ThenInclude(t => t!.Teams)
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

        /// <summary>
        /// Asynchronously retrieves the IDs of cards that are active in the first step.
        /// </summary>
        /// <remarks>A card is considered active in the first step if its associated step has an order
        /// value of 1.</remarks>
        /// <param name="cardIds">A collection of card IDs to filter. Only the IDs present in this collection and associated with cards in the
        /// first step will be returned.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of integers
        /// representing the IDs of the cards that are active in the first step.</returns>
        public async Task<ICollection<int>> FindActiveCardIdsInFirstStepAsync(IEnumerable<int> cardIds)
        {
            return await _context.Cards
                .Where(c => cardIds.Contains(c.Id) && c.Step!.Order == 1)
                .Select(c => c.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Finds all cards associated with a specific document ID.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A list of cards with their related Step information.</returns>
        public async Task<List<Card>> FindByDocumentIdAsync(int documentId)
        {
            return await _context.Cards
                .Where(c => c.DocumentId == documentId && c.Enable)
                .Include(c => c.Step)
                .Include(c => c.Outputs)
                    .ThenInclude(o => o.StepTool)
                        .ThenInclude(st => st!.Tool)
                .OrderBy(c => c.Step!.Order)
                .ToListAsync();
        }
    }
}
