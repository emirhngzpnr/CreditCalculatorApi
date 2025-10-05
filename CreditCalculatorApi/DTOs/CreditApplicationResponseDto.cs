using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.DTOs
{
    /// <summary>
    /// Represents the result of a credit application evaluation.
    /// </summary>
    public class CreditApplicationResponseDto
    {
        /// <summary>
        /// Unique identifier of the application.
        /// </summary>
        /// <example>101</example>
        public int Id { get; set; }

        /// <summary>
        /// Requested credit amount in Turkish Lira.
        /// </summary>
        /// <example>75000</example>
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// Credit term in months.
        /// </summary>
        /// <example>36</example>
        public int CreditTerm { get; set; }

        /// <summary>
        /// Applicant's declared monthly income in Turkish Lira.
        /// </summary>
        /// <example>25000</example>
        public decimal MonthlyIncome { get; set; }

        /// <summary>
        /// Risk status of the applicant based on evaluation (e.g., Low, Medium, High).
        /// </summary>
        /// <example>Low</example>
        public string RiskStatus { get; set; } = string.Empty;

        /// <summary>
        /// Suggested interest rate if applicable.
        /// </summary>
        /// <example>2.75</example>
        public double? InterestRate { get; set; }

        /// <summary>
        /// ID of the recommended campaign, if matched.
        /// </summary>
        /// <example>5</example>
        public int? CampaignId { get; set; }
        public CreditApplicationStatus Status { get; set; } = CreditApplicationStatus.Waiting;
        public DateTime RecordTime { get; set; }
        public string? CampaignName { get; set; }
        public string BankName { get; set; } = string.Empty;

    }
}
