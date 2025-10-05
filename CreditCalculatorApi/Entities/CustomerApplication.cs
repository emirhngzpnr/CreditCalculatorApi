using CreditCalculatorApi.DTOs;

namespace CreditCalculatorApi.Entities
{
    public class CustomerApplication
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SurName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }

        public string BankName { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        public CustomerApplicationStatus Status { get; set; } = CustomerApplicationStatus.Pending;
        public string CustomerNumber {  get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
