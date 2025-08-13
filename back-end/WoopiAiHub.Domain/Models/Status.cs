using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Status : BaseEntity
    {
        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; } = string.Empty;
        [Column("Color", TypeName = "varchar(7)")]
        public string Color { get; private set; } = string.Empty;

        public virtual ICollection<Step> Steps { get; set; } = [];
        public virtual ICollection<Card> Cards { get; set; } = [];

        public Status(string name, string color, int id, DateTime created) : base(id, created)
        {
            Name = name;
            Color = color;
        }

        private Status(int id, DateTime created) : base(id, created) { }
    }
}
