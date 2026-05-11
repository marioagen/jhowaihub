using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Permission : BaseEntity
    {
        [Column("Description", TypeName = "varchar(100)")]
        public string Description { get; private set; } = string.Empty;

        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; } = string.Empty;

        [Column("Group", TypeName = "varchar(50)")]
        public string Group { get; private set; }

        [Column("Active", TypeName = "bit")]
        public bool Active { get; private set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }
        public virtual ICollection<StepProfilePermission> StepProfilePermissions { get; set; }


        public Permission(string description,
                          string name,
                          string group,
                          int id,
                          DateTime created,
                          bool active = true) : base(id, created)
        {
            this.Description = description;
            this.Name = name;
            this.Group = group;
            this.Active = active;
        }

        private Permission(int id, DateTime created) : base(id, created) { }
    }
}

