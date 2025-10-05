using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.DTOs
{
    public class CreditApplicationStatusDto
    {
        public int ApplicationId { get; set; }
        public CreditApplicationStatus Status { get; set; }
    }
}
