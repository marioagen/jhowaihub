using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DocAnalyzer.Domain.Models
{
    public class TypeDoc : BaseEntity
    {
        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; }
        [Column("EmailCreator", TypeName = "varchar(50)")]
        public string EmailCreator { get; set; }
        [JsonIgnore]
        public virtual ICollection<Questionnaire> Questionnaires { get; set; }
        public TypeDoc( string name,
                        string emailCreator,
                        int id,
                        DateTime created) : base(id, created)
        {
            this.Name = name;
            this.EmailCreator = emailCreator;
        }

        private TypeDoc(int id, DateTime created) : base(id, created) { }
    }
}
