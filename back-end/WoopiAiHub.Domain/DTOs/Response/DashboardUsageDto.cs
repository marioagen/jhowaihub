namespace WoopiAiHub.Domain.DTOs.Response
{
    public class DashboardUsageDto
    {
        public string Date { get; set; }
        public int Value { get; set; }

        public DashboardUsageDto(string date, int value)
        {
            Date = date;
            Value = value;
        }
    }
}
