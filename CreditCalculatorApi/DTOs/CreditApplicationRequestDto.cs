using System.ComponentModel.DataAnnotations;
using CreditCalculatorApi.Entities.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace CreditCalculatorApi.DTOs
{
    /// <summary>
    /// Represents the data required to create a credit application.
    /// </summary>
    public class CreditApplicationRequestDto
    {
       
        public string FullName { get; set; }=string.Empty;

      
       
        public string Email { get; set; }=string.Empty;
      
       
        public string PhoneNumber { get; set; } = string.Empty; 


       
        public string BankName { get; set; }=string.Empty;

       
        public CreditType CreditType { get; set; }
     
        public int CampaignId { get; set; }

       
        public decimal CreditAmount { get; set; }

        
        public int CreditTerm { get; set; }

        public decimal MonthlyIncome { get; set; }
       

    }
}
