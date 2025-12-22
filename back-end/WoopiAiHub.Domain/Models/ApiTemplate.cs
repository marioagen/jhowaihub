using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    /// <summary>
    /// Entity that represents an API template for making HTTP requests.
    /// </summary>
    public class ApiTemplate
    {
        [Column("Id", TypeName = "uniqueidentifier")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("Created", TypeName = "datetime")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [Column("Name", TypeName = "varchar(200)")]
        public string Name { get; set; }

        [Column("Method", TypeName = "varchar(10)")]
        public string Method { get; set; }

        [Column("Url", TypeName = "varchar(100)")]
        public string Url { get; set; }

        [Column("QueryTemplate", TypeName = "varchar(max)")]
        public string? QueryTemplate { get; set; }

        [Column("HeaderTemplate", TypeName = "varchar(max)")]
        public string HeaderTemplate { get; set; }

        [Column("BodyTemplate", TypeName = "varchar(max)")]
        public string? BodyTemplate { get; set; }

        public ApiTemplate(string name, string method, string url, string? queryTemplate, string headerTemplate, string bodyTemplate)
        {
            Name = name;
            Method = method;
            Url = url;
            QueryTemplate = queryTemplate;
            HeaderTemplate = headerTemplate;
            BodyTemplate = bodyTemplate;
        }

        /// <summary>
        /// Use to EF context
        /// </summary>
        ApiTemplate() { }
    }
}
