using System.ComponentModel.DataAnnotations.Schema;
using WoopiAiHub.Domain.Constants;

namespace WoopiAiHub.Domain.Models
{
    /// <summary>
    /// Entity that represents an API template for making HTTP requests.
    /// </summary>
    public class ApiTemplate
    {
        [Column("Id", TypeName = "uniqueidentifier")]
        public Guid Id { get; private set; } = Guid.NewGuid();

        [Column("Created", TypeName = "datetime")]
        public DateTime Created { get; private set; } = DateTime.UtcNow;

        [Column("Name", TypeName = "varchar(200)")]
        public string Name { get; private set; }

        [Column("Method", TypeName = "varchar(10)")]
        public string Method { get; private set; }

        [Column("Url", TypeName = "varchar(100)")]
        public string Url { get; private set; }

        [Column("QueryTemplate", TypeName = "varchar(max)")]
        public string? QueryTemplate { get; private set; }

        [Column("HeaderTemplate", TypeName = "varchar(max)")]
        public string? HeaderTemplate { get; private set; }

        [Column("BodyTemplate", TypeName = "varchar(max)")]
        public string? BodyTemplate { get; private set; }


        public void UpdateName(string name) => Name = name;
        public void UpdateMethod(string method) => Method = method;
        public void UpdateUrl(string url) => Url = url;
        public void UpdateQueryTemplate(string? queryTemplate) => QueryTemplate = queryTemplate;
        public void UpdateHeaderTemplate(string? headerTemplate) => HeaderTemplate = headerTemplate;
        public void UpdateBodyTemplate(string? bodyTemplate) => BodyTemplate = bodyTemplate;

        public ApiTemplate(string name, string method, string url, string? queryTemplate, string? headerTemplate, string? bodyTemplate)
        {
            Name = name;
            Method = method;
            Url = url;
            QueryTemplate = queryTemplate;
            HeaderTemplate = headerTemplate;
            BodyTemplate = bodyTemplate;
            Validate();
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentException("Name cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(Method))
            {
                throw new ArgumentException("Method cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(Url))
            {
                throw new ArgumentException("Url cannot be null or empty.");
            }
            if(
                Method != HttpMethodConstants.GET && 
                Method != HttpMethodConstants.POST &&
                Method != HttpMethodConstants.PUT &&
                Method != HttpMethodConstants.DELETE &&
                Method != HttpMethodConstants.PATCH
            )
            {
                throw new ArgumentException("Method must be a valid HTTP method.");
            }
        }
    }
}
