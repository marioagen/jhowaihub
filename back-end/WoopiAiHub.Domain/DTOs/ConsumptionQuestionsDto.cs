namespace WoopiAiHub.Domain.DTOs
{
    public class ConsumptionQuestionsDto
    {
        public string Email { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;

        /// <summary>
        /// Boolean to check if the dto is originally from services (by key)
        /// </summary>
        public bool IsKeyOrigin { get; set; } = false;
        
    }
}
