namespace CreditCalculatorApi.Events
{
    public class AppLogEvent
    {
        public string LogType { get; set; } = "Info";  // Info, Error, Warning...
        public string Message { get; set; } = "";
        public string? UserId { get; set; }
        public string? Source { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public object? Data { get; set; }
        public string? Exception { get; set; }
        public string? CorrelationId { get; set; }
    }
}
