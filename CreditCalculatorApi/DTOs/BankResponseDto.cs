namespace CreditCalculatorApi.DTOs
{
    public class BankResponseDto
    {
        public int Id { get; set; }
        public DateTime RecordTime { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = string.Empty;
    }
}
