using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Repository.Interfaces
{
    public interface ICampaignRepository
    {
        Task<List<Campaign>> GetAllWithBanksAsync();
        Task<Campaign?> GetByIdAsync(int id);         
        Task AddAsync(Campaign campaign);            
        Task UpdateAsync(Campaign campaign);         
        Task DeleteAsync(Campaign campaign);
        Task<List<Campaign>> GetActiveCampaignsWithBanksAsync();
        Task<List<Campaign>> GetAllAsync(); // tüm kampanyaları çeker


    }
}
