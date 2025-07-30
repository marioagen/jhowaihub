using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Permission : BaseEntity
    {
        [Column("Description", TypeName = "varchar(100)")]
        public string Description { get; private set; } = string.Empty;

        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; } = string.Empty;

        [Column("Module", TypeName = "varchar(50)")]
        public string Module { get; private set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }

        public Permission(string description,
                          string name,
                          string module,
                          int id,
                          DateTime created) : base(id, created)
        {
            this.Description = description;
            this.Name = name;
            this.Module = module;
        }

        private Permission(int id, DateTime created) : base(id, created) { }
    }
}

