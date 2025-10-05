namespace CreditCalculatorApi.DTOs.Logs
{
    public class LogsQueryDto
    {
        // "Info" | "Warning" | "Error" | (boş = hepsi)
        public string? Level { get; set; }

        // Mesaj/kaynak/exception içinde arama
        public string? Search { get; set; }

        // UTC aralığı
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }

        // Sayfalama
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
