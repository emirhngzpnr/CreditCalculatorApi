namespace CreditCalculatorApi.Events
{
    public class RiskEvaluatedEvent
    {
        public int ApplicationId { get; set; }
        public string UserNumber { get; set; } = default!;
        public decimal MonthlyIncome { get; set; }
        public decimal MonthlyInstallment { get; set; } // Hesaplanmış taksit
        public decimal Dti { get; set; }                 // taksit/gelir
        public string RiskLabel { get; set; } = "";      // safe | medium | risky
        public int BankId { get; set; }
        public int? CampaignId { get; set; }
        public decimal Amount { get; set; }
        public int Term { get; set; }
        public DateTime Ts { get; set; } = DateTime.UtcNow;
    }
}
