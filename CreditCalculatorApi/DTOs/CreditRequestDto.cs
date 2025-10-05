using System.ComponentModel.DataAnnotations;

namespace CreditCalculatorApi.DTOs
{
    /// <summary>
    /// Represents the required data for calculating a loan payment plan.
    /// </summary>
    public class CreditRequestDto
    {
       
        public decimal KrediTutari { get; set; }

     
        public int Vade { get; set; }

      
        public decimal FaizOrani { get; set; }
    }
}
