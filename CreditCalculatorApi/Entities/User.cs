using CreditCalculatorApi.Entities.Enums;

namespace CreditCalculatorApi.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty; // Ad
        public string LastName { get; set; } = string.Empty;  // Soyad
        public string Email { get; set; } = string.Empty; // E-mail
        public string PhoneNumber { get; set; } = string.Empty; // Telefon numarası
        public string IdentityNumberEncrypted { get; set; } = string.Empty; // T.C. Kimlik Numarası
        public DateTime BirthDate { get; set; }

        public string PasswordHash { get; set; } = string.Empty; // Şifre Hash'i
        public bool IsEmailConfirmed { get; set; }=false;
        public string AesKeyEncrypted { get; set; } = string.Empty;
        public string AesIVEncrypted { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserNumber { get; set; } = string.Empty; // Kullanıcı numarası ( 12 haneli ve site kullanıcısı 10 ile başlar)
        public UserRole Role { get; set; } = UserRole.User;

    }
}
