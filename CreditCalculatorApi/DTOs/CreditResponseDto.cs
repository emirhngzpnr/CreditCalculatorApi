namespace CreditCalculatorApi.DTOs
{
    /// <summary>
    /// Represents the result of a credit calculation including payment details and installment breakdown.
    /// </summary>
    public class CreditResponseDto
    {
        /// <summary>
        /// The amount to be paid monthly.
        /// </summary>
        /// <example>2350.75</example>
        public decimal MonthlyPayment { get; set; }

        /// <summary>
        /// The total amount to be paid at the end of the credit term.
        /// </summary>
        /// <example>84627.00</example>
        public decimal TotalPayment { get; set; }

        /// <summary>
        /// List of monthly installments.
        /// </summary>
        public List<InstallmentDto> Installments { get; set; } = new();
    }
}
