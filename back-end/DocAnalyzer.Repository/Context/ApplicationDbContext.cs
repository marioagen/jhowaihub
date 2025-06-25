using DocAnalyzer.Domain.Models;
using DocAnalyzer.Repository.Mappings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DocAnalyzer.Repository.Context
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            IHttpContextAccessor httpContextAccessor,
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
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
            base.OnModelCreating(modelBuilder);
        }
    }
}
