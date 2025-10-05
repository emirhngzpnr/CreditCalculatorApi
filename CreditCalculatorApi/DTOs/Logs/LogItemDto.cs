namespace CreditCalculatorApi.DTOs.Logs
{
    public class LogItemDto
    {
        public string LogType { get; set; } = "Info";
        public string Message { get; set; } = string.Empty;
        public string? Source { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? CorrelationId { get; set; }
        public string? Exception { get; set; }
        public object? Data { get; set; }
    }
}
