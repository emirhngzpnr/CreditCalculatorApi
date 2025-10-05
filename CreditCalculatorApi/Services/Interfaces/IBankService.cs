using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.DTOs.Reports;
using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Services.Interfaces
{
    public interface IBankService
    {
        Task<List<BankResponseDto>> GetAllBanksAsync();
        Task<BankResponseDto> AddBankAsync(BankRequestDto dto);
        Task UpdateBankAsync(int id,BankRequestDto dto);
        Task<int> GetBankIdByNameAsync(string bankName);
        
        Task DeleteBankAsync(int id);

    }
}
