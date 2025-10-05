using CreditCalculatorApi.DTOs;

namespace CreditCalculatorApi.Services.Interfaces
{
    public interface ICustomerApplicationService
    {
        
            Task<CustomerApplicationResponseDto> CreateAsync(CustomerApplicationRequestDto dto);
        Task<List<CustomerApplicationResponseDto>> GetByBankNameAsync(string bankName);
        Task UpdateStatusAsync(int id, CustomerApplicationStatus newStatus);
        Task<bool> IsCustomerAsync(string email, string bankName);
        Task<List<CustomerApplicationResponseDto>> GetByUserIdAsync(string userId);
        Task<List<CustomerApplicationResponseDto>> GetApprovedMembershipsAsync(string userId);
   

        Task DeleteAsync(int id);



    }
}
