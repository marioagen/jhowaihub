using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class Prompt : BaseEntity
    {
        [Column("Name", TypeName = "varchar(50)")]
        public string Name { get; private set; } = string.Empty;

        [Column("Description", TypeName = "varchar(95)")]
        public string Description { get; private set; } = string.Empty;

        [Column("Text", TypeName = "nvarchar(max)")]
        public string Text { get; private set; } = string.Empty;

        [Column("IdUser", TypeName = "uniqueIdentifier")]
        public Guid IdUser { get; private set; } = Guid.Empty;

        [Column("IsEdited", TypeName = "bit")] 
        public bool IsEdited { get; private set; } = false;

        [Column("IsImported", TypeName = "bit")]
        public bool IsImported { get; private set; } = false;

        public virtual User User { get; set; }

        public Prompt(int id, DateTime created, string name, string description, string text, Guid idUser,
            bool isEdited = false, bool isImported = false)
            : base(id, created)
        {
            Name = name;
            Description = description;
            Text = text;
            IdUser = idUser;
            IsEdited = isEdited;
            IsImported = isImported;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        private Prompt(int id, DateTime created) : base(id, created) { }
    }
}
