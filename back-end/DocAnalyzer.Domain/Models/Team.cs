using System.ComponentModel.DataAnnotations.Schema;

namespace DocAnalyzer.Domain.Models
{
    public class Team : BaseEntity
    {
        [Column("Name", TypeName = "varchar(100)")]
        public string Name { get; private set; } = string.Empty;

        public virtual ICollection<User>? Users { get; set; }

        public Team(string name,
                    int id,
                    DateTime created) : base(id, created)
        {
            this.Name = name;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Team(int id, DateTime created) : base(id, created) { }
    }
}
