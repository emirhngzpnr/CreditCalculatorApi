using System.ComponentModel.DataAnnotations;


namespace CreditCalculatorApi.DTOs
{
    /// <summary>
    /// Represents the information required to create a new customer application.
    /// </summary>
    public class CustomerApplicationRequestDto
    {

        public string Name { get; set; } = string.Empty;

        public string SurName { get; set; } = string.Empty;

        public string IdentityNumber { get; set; } = string.Empty;

       
        public string Phone { get; set; } = string.Empty;

      
        public string Email { get; set; } = string.Empty;

        
        public DateTime BirthDate { get; set; }

     
        public string BankName { get; set; } = string.Empty;
    }
}
