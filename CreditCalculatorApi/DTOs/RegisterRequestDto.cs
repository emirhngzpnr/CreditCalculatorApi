namespace CreditCalculatorApi.DTOs
{
    public class RegisterRequestDto
    {
        public string FirstName { get; set; } = string.Empty; // Ad
        public string LastName { get; set; } = string.Empty;  // Soyad
        public string Email { get; set; } = string.Empty; // E-mail
        public string PhoneNumber { get; set; } = string.Empty; // Telefon numarası
        public DateTime BirthDate { get; set; } // Doğum tarihi
        public string IdentityNumber { get; set; } = string.Empty; // T.C. Kimlik Numarası
        public string Password { get; set; } = string.Empty; // Şifre

    }
}
