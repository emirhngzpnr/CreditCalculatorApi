namespace CreditCalculatorApi.DTOs
{
    /// <summary>
    /// Represents the response data returned after a customer application is created.
    /// </summary>
    public class CustomerApplicationResponseDto
    {
        /// <summary>
        /// Unique identifier of the customer application.
        /// </summary>
        /// <example>2001</example>
        public int Id { get; set; }

        /// <summary>
        /// First name of the applicant.
        /// </summary>
        /// <example>Ahmet</example>
        public string Name { get; set; }

        /// <summary>
        /// Last name of the applicant.
        /// </summary>
        /// <example>Yılmaz</example>
        public string SurName { get; set; }

        /// <summary>
        /// Name of the bank the application was submitted to.
        /// </summary>
        /// <example>Garanti BBVA</example>
        public string BankName { get; set; }
     

        /// <summary>
        /// Date and time when the application was created.
        /// </summary>
        /// <example>2025-07-21T14:25:00</example>
        public DateTime ApplicationDate { get; set; }

        public CustomerApplicationStatus Status { get; set; }= CustomerApplicationStatus.Pending;
        public string CustomerNumber { get; set; }


    }
}
