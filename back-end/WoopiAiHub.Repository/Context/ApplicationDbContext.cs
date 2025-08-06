using WoopiAiHub.Domain.Models;
using WoopiAiHub.Repository.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace WoopiAiHub.Repository.Context
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            IHttpContextAccessor httpContextAccessor,
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            _httpContextAccessor = null!;
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentHistory> DocumentHistories { get; set; }
        public DbSet<DocumentNormalized> DocumentNormalized { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionQuestionnaire> QuestionQuestionnaire { get; set; }
        public DbSet<TypeDoc> TypeDoc { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<Step> Steps { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<StepStatus> StepStatuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_httpContextAccessor?.HttpContext?.Items["TenantConnection"] is string connectionString &&
                !string.IsNullOrWhiteSpace(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>(new DocumentMap().Configure);
            modelBuilder.Entity<DocumentHistory>(new DocumentHistoryMap().Configure);
            modelBuilder.Entity<DocumentNormalized>(new DocumentNormalizedMap().Configure);
            modelBuilder.Entity<Questionnaire>(new QuestionnaireMap().Configure);
            modelBuilder.Entity<Question>(new QuestionMap().Configure);
            modelBuilder.Entity<QuestionQuestionnaire>(new QuestionQuestionnaireMap().Configure);
            modelBuilder.Entity<TypeDoc>(new TypeDocMap().Configure);
            modelBuilder.Entity<User>(new UserMap().Configure);
            modelBuilder.Entity<Team>(new TeamMap().Configure);
            modelBuilder.Entity<Permission>(new PermissionMap().Configure);
            modelBuilder.Entity<Profile>(new ProfileMap().Configure);
            modelBuilder.Entity<Workflow>(new WorkflowMap().Configure);
            modelBuilder.Entity<Step>(new StepMap().Configure);
            modelBuilder.Entity<Card>(new CardMap().Configure);
            modelBuilder.Entity<StepStatus>(new StepStatusMap().Configure);
            base.OnModelCreating(modelBuilder);
        }
    }
}
