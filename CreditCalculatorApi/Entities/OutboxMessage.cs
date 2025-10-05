namespace CreditCalculatorApi.Entities
{
    public class OutboxMessage
    {
        public long Id { get; set; }
        public string Type { get; set; } = "";
        public string Payload { get; set; } = "";
        public string AggregateId { get; set; } = "";
        public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedOnUtc { get; set; }
        public string? Error { get; set; }
    }
}
