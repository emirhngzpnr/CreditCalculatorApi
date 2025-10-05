using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Services.Account
{
    public interface IAccountService
    {
        Task RegisterAsync(RegisterRequestDto dto);
        Task<User?> GetByEmailAsync(string email);
        Task ConfirmEmailAsync(string token);

    }
}
