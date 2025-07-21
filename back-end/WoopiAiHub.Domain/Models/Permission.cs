using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Permission : BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; } = string.Empty;

        public virtual ICollection<User?> Users { get; set; }
        public virtual ICollection<Profile?> Profiles { get; set; }

        public Permission(string name,
                       int id,
                       DateTime created) : base(id, created)
        {
            this.Name = name;
        }

        private Permission(int id, DateTime created) : base(id, created) { }
    }
}

