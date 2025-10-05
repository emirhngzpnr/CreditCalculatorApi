using CreditCalculatorApi.DTOs;

namespace CreditCalculatorApi.Services.Auth
{
    public interface IAuthService
    {
       
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task SendResetPasswordEmailAsync(string email);
        Task ResetPasswordAsync(string token, string newPassword);


    }
}
