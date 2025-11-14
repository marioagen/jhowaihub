using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Tenant : BaseEntity
    {
        [Column("MarketplaceSubscriptionId", TypeName = "uniqueidentifier")]
        public Guid MarketplaceSubscriptionId { get; private set; }
        [Column("Name", TypeName = "varchar(255)")]
        public string? Name { get; private set; }
        [Column("IsActive", TypeName = "bit")]
        public bool IsActive { get; private set; }
        [Column("PlanName", TypeName = "varchar(50)")]
        public string? PlanName { get; private set; }
        [Column("DateStartSubscription", TypeName = "datetime")]
        public DateTime? DateStartSubscription { get; private set; }
        [Column("DateEndSubscription", TypeName = "datetime")]
        public DateTime? DateEndSubscription { get; private set; }
        [Column("DateRenewSubscription", TypeName = "datetime")]
        public DateTime? DateRenewSubscription { get; private set; }
        [Column("KeyAiGateway", TypeName = "varchar(255)")]
        public string? KeyAiGateway { get; private set; }

        // Construtor completo
        public Tenant(int id,
                      DateTime created,
                      string name,
                      Guid marketplaceSubscriptionId,
                      bool isActive,
                      string planName,
                      DateTime? dateStartSubscription,
                      DateTime? dateEndSubscription,
                      DateTime? dateRenewSubscription,
                     string keyAiGateway) : base(id, created)
        {
            Name = name;
            MarketplaceSubscriptionId = marketplaceSubscriptionId;
            Name = name;
            IsActive = isActive;
            PlanName = planName;
            DateStartSubscription = dateStartSubscription;
            DateEndSubscription = dateEndSubscription;
            DateRenewSubscription = dateRenewSubscription;
            KeyAiGateway = keyAiGateway;
        }

        // Construtor apenas com os parâmetros da BaseEntity
        public Tenant(int id, DateTime created) : base(id, created) { }

        public void SetActive(bool value)
        {
            IsActive = value;
        }

        public void SetSubscriptionDates(DateTime? startDate,
                                         DateTime? endDate,
                                         DateTime? renewDate)
        {
            DateStartSubscription = startDate;
            DateEndSubscription = endDate;
            DateRenewSubscription = renewDate;
        }

        public void SetPlanName(string planName)
        {
            PlanName = planName;
        }
    }
}
