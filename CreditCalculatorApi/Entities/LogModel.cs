using System.ComponentModel.DataAnnotations.Schema;

namespace CreditCalculatorApi.Entities
{
    [Table("AppLogs")]
    public class LogModel
    {
        public int Id { get; set; }

        public string LogType { get; set; } = "Info"; // Info, Warning, Error, Debug, Critical
        public string Message { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public string? Source { get; set; } // CreditService, CreditController vs.

        public string? Exception { get; set; } // varsa stack trace veya ex.Message
        public string? Data { get; set; } // ekstra JSON verisi saklamak için

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
