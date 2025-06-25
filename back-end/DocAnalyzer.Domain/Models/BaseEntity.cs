using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocAnalyzer.Domain.Models
{
    public class BaseEntity
    {
        [Key]
        [Column("Id", TypeName = "int")]
        public int Id { get; private set; }

        [Column("Created", TypeName = "datetime")]
        public DateTime Created { get; private set; }

        public BaseEntity(int id, DateTime created)
        {
            this.Id = id;
            this.Created = created;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private BaseEntity() { }
    }
}
