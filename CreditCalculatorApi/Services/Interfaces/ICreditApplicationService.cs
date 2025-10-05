using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Services.Interfaces
{
    public interface ICreditApplicationService
    {
        Task<List<CreditApplication>> GetAllApplicationsAsync();
        Task<CreditApplication> CreateApplicationAsync(CreditApplicationRequestDto application);
        Task<List<object>> GetApplicationReportAsync();
        Task UpdateStatusAsync(int id, CreditApplicationStatus newStatus);
        Task DeleteApplicationAsync(int id);
        Task<List<CreditApplicationResponseDto>> GetByUserEmailAsync(string email);

        Task<List<CreditApplicationResponseDto>> GetApprovedCreditsAsync(string userId);





    }
}
