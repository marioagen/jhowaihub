using Microsoft.EntityFrameworkCore;
using WoopiAiHub.Domain.Enum.Audit;
using WoopiAiHub.Domain.Models.Audit;
using WoopiAiHub.Repository.Audit;
using WoopiAiHub.Repository.Context;
using Xunit;

namespace WoopiAiHub.UnitTests.Audit
{
    public class AuditCardRepositoryTests
    {
        private static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact(DisplayName = "Add should attach AuditCard to context")]
        [Trait("AuditCardRepository", "Add")]
        public void Add_ShouldAttachAuditCardToContext()
        {
            using var context = CreateContext();
            var repository = new AuditCardRepository(context);

            var occurredAt = DateTime.UtcNow;
            var auditCard = new AuditCard(0, occurredAt, cardId: 1, workflowId: 1, AuditCardActionType.Assign, Guid.NewGuid());

            repository.Add(auditCard);

            var entry = context.Entry(auditCard);
            Assert.Equal(EntityState.Added, entry.State);
        }

        [Fact(DisplayName = "Add then SaveChanges should persist AuditCard")]
        [Trait("AuditCardRepository", "Add")]
        public void Add_ThenSaveChanges_ShouldPersistAuditCard()
        {
            using var context = CreateContext();
            var repository = new AuditCardRepository(context);

            var userId = Guid.NewGuid();
            var occurredAt = DateTime.UtcNow;
            var auditCard = new AuditCard(0, occurredAt, cardId: 1, workflowId: 1, AuditCardActionType.Advancement, userId);

            repository.Add(auditCard);
            context.SaveChanges();

            var saved = context.Set<AuditCard>().FirstOrDefault(a => a.CardId == 1 && a.WorkflowId == 1 && a.UserId == userId);
            Assert.NotNull(saved);
            Assert.Equal(1, saved.CardId);
            Assert.Equal(1, saved.WorkflowId);
            Assert.Equal(AuditCardActionType.Advancement, saved.ActionType);
            Assert.Equal(userId, saved.UserId);
        }

        [Fact(DisplayName = "Add should allow multiple AuditCards before SaveChanges")]
        [Trait("AuditCardRepository", "Add")]
        public void Add_ShouldAllowMultipleAuditCardsBeforeSaveChanges()
        {
            using var context = CreateContext();
            var repository = new AuditCardRepository(context);

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            repository.Add(new AuditCard(0, DateTime.UtcNow, 1, 1, AuditCardActionType.Assign, guid1));
            repository.Add(new AuditCard(0, DateTime.UtcNow, 2, 1, AuditCardActionType.Unassign, guid2));

            context.SaveChanges();

            var count = context.Set<AuditCard>().Count();
            Assert.Equal(2, count);
        }
    }
}
