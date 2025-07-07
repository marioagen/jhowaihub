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
        public string Name { get; set; } = string.Empty;

        [Column("Email", TypeName = "varchar(256)")]
        public string Email { get; set; } = string.Empty;

        [Column("IsActive", TypeName = "bit")]
        public bool IsActive { get; set; }

        [Column("Created", TypeName = "datetime")]
        public DateTime Created { get; private set; }

        public ICollection<Team> Teams { get; set; } = new Collection<Team>();

        public User(Guid id,
                    string name,
                    string email,
                    bool isActive,
                    DateTime created)
        {
            this.Id = id;
            this.Name = name;
            this.Email = email;
            this.IsActive = isActive;
            this.Created = created;
        }

        public void AddTeam(Team team)
        {
            if (team == null)
                throw new ArgumentNullException(nameof(team));

            if (this.Teams.Any(t => t.Id == team.Id))
                return;

            Teams.Add(team);
        }
    }
}
