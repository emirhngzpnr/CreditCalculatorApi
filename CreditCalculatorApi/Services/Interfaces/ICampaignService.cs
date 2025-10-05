using CreditCalculatorApi.DTOs;

namespace CreditCalculatorApi.Services.Interfaces
{
    public interface ICampaignService
    {
        Task<List<CampaignResponseDto>> GetAllCampaignsAsync();
        Task AddCampaignAsync(CampaignRequestDto dto);
        Task UpdateCampaignAsync(int id, CampaignRequestDto dto);
        Task DeleteCampaignAsync(int id);
        Task<List<CampaignResponseDto>> GetCampaignsByBankAndTypeAsync(string bankName, int creditType);
        Task UpdateExpiredCampaignStatusesAsync();

    }
}
