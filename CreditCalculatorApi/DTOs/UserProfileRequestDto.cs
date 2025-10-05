namespace CreditCalculatorApi.DTOs
{
    public class UserProfileRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
       
        public DateTime BirthDate { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
    }
}
