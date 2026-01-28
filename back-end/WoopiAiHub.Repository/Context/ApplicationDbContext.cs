using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WoopiAiHub.Domain.Models;
using WoopiAiHub.Domain.Utils;
using WoopiAiHub.Repository.Mappings;
using WoopiAiHub.Repository.Util;

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
        public DbSet<Status> Status { get; set; }
        public DbSet<ToolData> ToolDatas { get; set; }
        public DbSet<ToolType> ToolTypes { get; set; }
        public DbSet<Tool> Tools { get; set; }
        public DbSet<StepTool> StepTools { get; set; }
        public DbSet<StepToolExecution> StepToolExecutions { get; set; }
        public DbSet<StepToolOutput> StepToolOutputs { get; set; }
        public DbSet<StepToolParameter> StepToolParameters { get; set; }
        public DbSet<StepToolDependency> StepToolDependencies { get; set; }
        public DbSet<StepProfilePermission> StepProfilePermissions { get; set; }
        public DbSet<Prompt> Prompts { get; set; }
        public DbSet<UsageLog> UsageLogs { get; set; }
        public DbSet<UsageMonth> UsageMonths { get; set; }
        public DbSet<ModelEmbedding> ModelEmbeddings { get; set; }
        public DbSet<UsageType> UsageTypes { get; set; }
        public DbSet<UsageUnit> UsageUnits { get; set; }
        public DbSet<UsageDaily> UsageDailies { get; set; }
        public DbSet<ApiTemplate> ApiTemplates { get; set; }
        public DbSet<SubscriptionPeriod> SubscriptionPeriods { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

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
            modelBuilder.Entity<Status>(new StatusMap().Configure);
            modelBuilder.Entity<ToolData>(new ToolDataMap().Configure);
            modelBuilder.Entity<ToolType>(new ToolTypeMap().Configure);
            modelBuilder.Entity<Tool>(new ToolMap().Configure);
            modelBuilder.Entity<StepTool>(new StepToolMap().Configure);
            modelBuilder.Entity<StepToolExecution>(new StepToolExecutionMap().Configure);
            modelBuilder.Entity<StepToolParameter>(new StepToolParameterMap().Configure);
            modelBuilder.Entity<StepToolOutput>(new StepToolOutputMap().Configure);
            modelBuilder.Entity<StepToolDependency>(new StepToolDependencyMap().Configure);
            modelBuilder.Entity<StepProfilePermission>(new StepProfilePermissionMap().Configure);
            modelBuilder.Entity<Prompt>(new PromptMap().Configure);
            modelBuilder.Entity<UsageDaily>(new UsageDailyMap().Configure);
            modelBuilder.Entity<UsageMonth>(new UsageMonthMap().Configure);
            modelBuilder.Entity<UsageLog>(new UsageLogMap().Configure);
            modelBuilder.Entity<ModelEmbedding>(new ModelEmbeddingsMap().Configure);
            modelBuilder.Entity<UsageUnit>(new UsageUnitMap().Configure);
            modelBuilder.Entity<UsageType>(new UsageTypeMap().Configure);
            modelBuilder.Entity<ApiTemplate>(new ApiTemplateMap().Configure);
            modelBuilder.Entity<SubscriptionPeriod>(new SubscriptionPeriodMap().Configure);
            modelBuilder.Entity<AuditLog>(new AuditLogMap().Configure);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        public override int SaveChanges()
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges();
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditLog> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditLogs = new List<AuditLog>();

            var user = GetCurrentUser();
            if (user == null)
                return auditLogs;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (AuditExtensions.ShouldSkipEntry(entry))
                    continue;

                var auditLog = AuditExtensions.CreateAuditLogFromEntry(entry, user);
                if (auditLog != null)
                {
                    auditLogs.Add(auditLog);
                }
            }

            return auditLogs;
        }

        private User? GetCurrentUser()
        {
            var requestEmail = _httpContextAccessor?.HttpContext?.Request.Headers[HeaderNames.XEmail].ToString();
            if (string.IsNullOrEmpty(requestEmail))
                return null;

            return Users.FirstOrDefault(u => u.Email == requestEmail);
        }

        private void OnAfterSaveChanges(List<AuditLog> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            Set<AuditLog>().AddRange(auditEntries);
            base.SaveChanges();
        }
    }
}
