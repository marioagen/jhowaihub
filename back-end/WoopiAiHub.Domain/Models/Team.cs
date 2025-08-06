using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Team : BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; } = string.Empty;

        public virtual ICollection<User> Users { get; set; } = [];
        public virtual ICollection<Document> Documents { get; set; } = [];
        public virtual Workflow? Workflow { get; set; }

        public Team(string name,
                    int id,
                    DateTime created) : base(id, created)
        {
            this.Name = name;
        }

        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (this.Users.Any(t => t.Id == user.Id))
                return;

            Users.Add(user);
        }

        public void Update(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Team(int id, DateTime created) : base(id, created) { }
    }
}
