namespace CreditCalculatorApi.DTOs
{
    public class UserProfileResponseDto
    {
        public string UserNumber { get; set; } = string.Empty; // Kullanıcı numarası (12 haneli ve site kullanıcısı 10 ile başlar)
        public string FirstName { get; set; } = string.Empty; // Ad
        public string LastName { get; set; } = string.Empty;  // Soyad
        public string Email { get; set; } = string.Empty; // E-mail
        public string PhoneNumber { get; set; } = string.Empty; // Telefon numarası
        public DateTime BirthDate { get; set; } // Doğum tarihi
        public string IdentityNumber { get; set; } = string.Empty; // T.C. Kimlik Numarası
   
    }
}
