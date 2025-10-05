using Microsoft.AspNetCore.Mvc;

namespace CreditCalculatorApi.DTOs
{
  
    public class AuthResponseDto
    {
        public string FirstName { get; set; } = string.Empty; // Ad
        public string LastName { get; set; } = string.Empty; // Soyad
        public string Email { get; set; } = string.Empty; // E-mail
        public string Token { get; set; } = string.Empty; // JWT Token

        public DateTime ExpiresAt { get; set; }  // Token'ın geçerlilik süresi
    }
}
