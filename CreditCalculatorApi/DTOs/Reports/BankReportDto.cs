namespace CreditCalculatorApi.DTOs.Reports
{
    public class BankReportDto
    {
        public int BankId { get; set; }
        public string BankName { get; set; }
        public int TotalApplications { get; set; }
        public decimal TotalCreditAmount { get; set; }
        public decimal AverageCreditAmount { get; set; }
    }
}
