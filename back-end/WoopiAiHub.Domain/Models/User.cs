using AutoMapper;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class User
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; private set; }

        [Column("Name", TypeName = "varchar(150)")]
        public string Name { get; private set; } = string.Empty;

        [Column("Email", TypeName = "varchar(256)")]
        public string Email { get; private set; } = string.Empty;

        [Column("IsActive", TypeName = "bit")]
        public bool IsActive { get; private set; }

        [Column("PasswordHash", TypeName = "varbinary(64)")]
        public byte[] PasswordHash { get; private set; } = Array.Empty<byte>();

        [Column("Salt", TypeName = "varbinary(16)")]
        public byte[] Salt { get; private set; } = Array.Empty<byte>();

        [Column("Created", TypeName = "datetime")]
        public DateTime Created { get; private set; }

        [Column("LastLoginAt", TypeName = "datetime")]
        public DateTime? LastLoginAt { get; private set; }

        public virtual ICollection<Permission> Permissions { get; set; } = [];

        public virtual ICollection<Prompt> Prompts { get; set; } = [];
        public ICollection<Team> Teams { get; set; } = [];

        public virtual ICollection<UsageDaily> UsageDailies { get; set; } = [];
        public virtual ICollection<UsageLog> UsageLogs { get; set; } = [];
        public virtual ICollection<UsageMonth> UsageMonths { get; set; } = [];
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = [];
        public virtual ICollection<DocumentHistory> DocumentHistories { get; set; } = [];

        public User(Guid id,
                    string name,
                    string email,
                    bool isActive,
                    DateTime created)
        {
            Id = id;
            Name = name;
            Email = email;
            IsActive = isActive;
            Created = created;
        }

        public void AddTeam(Team team)
        {
            ArgumentNullException.ThrowIfNull(nameof(team));

            if (this.Teams.Any(t => t.Id == team.Id))
                return;

            Teams.Add(team);
        }

        public void Reactivate(string name,
                               string email)
        {
            IsActive = true;
            Name = name;
            Email = email;
            Created = DateTime.Now;
        }

        public void Update(string name,
                           string email)
        {
            Name = name;
            Email = email;
        }

        public void Deactivate()
        {
            IsActive = false;
            Teams.Clear();
        }

        public void SetPassword(byte[] passwordHash, byte[] salt)
        {
            ArgumentNullException.ThrowIfNull(nameof(passwordHash));
            ArgumentNullException.ThrowIfNull(nameof(salt));

            PasswordHash = passwordHash;
            Salt = salt;
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }
    }
}
