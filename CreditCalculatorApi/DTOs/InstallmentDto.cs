namespace CreditCalculatorApi.DTOs
{
    /// <summary>
    /// Represents a single installment in a loan payment schedule.
    /// </summary>
    public class InstallmentDto
    {
        /// <summary>
        /// The sequential number of the installment.
        /// </summary>
        /// <example>1</example>
        public int InstallmentNo { get; set; }

        /// <summary>
        /// Total payment amount for the installment.
        /// </summary>
        /// <example>2500.75</example>
        public decimal Payment { get; set; }

        /// <summary>
        /// Interest portion of the installment payment.
        /// </summary>
        /// <example>750.25</example>
        public decimal Interest { get; set; }

        /// <summary>
        /// Principal (loan repayment) portion of the installment payment.
        /// </summary>
        /// <example>1750.50</example>
        public decimal Principal { get; set; }

        /// <summary>
        /// Remaining principal after this installment is paid.
        /// </summary>
        /// <example>48249.50</example>
        public decimal RemainingPrincipal { get; set; }
    }
}
