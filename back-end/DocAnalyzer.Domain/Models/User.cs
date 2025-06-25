using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocAnalyzer.Domain.Models
{
    public class User
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; private set; }

        [Column("Name", TypeName = "varchar(150)")]
        public string Name { get; private set; } = string.Empty;

        [Column("Email", TypeName = "varchar(254)")]
        public string Email { get; private set; } = string.Empty;

        [Column("IsActive", TypeName = "bit")]
        public bool IsActive { get; private set; }

        [Column("Created", TypeName = "datetime")]
        public DateTime Created { get; private set; }

        public ICollection<Team>? Teams { get; set; }

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
    }
}
