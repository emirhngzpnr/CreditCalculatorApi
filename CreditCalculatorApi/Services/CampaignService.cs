using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Entities.Enums;
using CreditCalculatorApi.Monitoring;
using CreditCalculatorApi.Repository.Interfaces;
using CreditCalculatorApi.Services.Interfaces;

namespace CreditCalculatorApi.Services
{
    public class CampaignService:ICampaignService
    {
        private readonly ICampaignRepository _repository;
        private readonly ILogger<CampaignService> _logger;
        private readonly ILogService _logService;

        public CampaignService(ICampaignRepository repository, ILogger<CampaignService> logger, ILogService logService)
        {
            _repository = repository;
            _logger = logger;
            _logService = logService;
        }

        public async Task<List<CampaignResponseDto>> GetAllCampaignsAsync()
        {
            try
            {
                var campaigns = await _repository.GetActiveCampaignsWithBanksAsync();

                _logger.LogInformation("Toplam {count} kampanya bulundu.", campaigns.Count);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Toplam {campaigns.Count} kampanya listelendi.",
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });

                foreach (var c in campaigns)
                {
                    var now = DateTime.UtcNow.Date;
                    var expectedStatus = c.BitisTarihi < now ? CampaignStatus.Expired : CampaignStatus.Active;

                    if (c.CampaignStatus != expectedStatus)
                    {
                        c.CampaignStatus = expectedStatus;
                        await _repository.UpdateAsync(c);
                        _logger.LogInformation("Kampanya status güncellendi. Id={Id}, Yeni Status={Status}", c.Id, expectedStatus);
                        await _logService.LogAsync(new LogModel
                        {
                            LogType = "Info",
                            Message = $"Kampanya statüsü güncellendi. Id={c.Id}, Yeni: {expectedStatus}",
                            Source = "CampaignService",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                return campaigns.Select(c => new CampaignResponseDto
                {
                    Id = c.Id,
                    CreditType = c.CreditType.ToString(),
                    MinVade = c.MinVade,
                    MaxVade = c.MaxVade,
                    MinKrediTutar = c.MinKrediTutar,
                    MaxKrediTutar = c.MaxKrediTutar,
                    BaslangicTarihi = c.BaslangicTarihi,
                    BitisTarihi = c.BitisTarihi,
                    Description = c.Description,
                    FaizOrani = c.FaizOrani,
                    BankId = c.BankId,
                    BankName = c.Bank?.Name ?? "Unknown"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kampanyalar getirilirken hata oluştu");
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Kampanyalar getirilirken hata oluştu: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("An error occurred while processing campaign data", ex);
            }
        }

        public async Task AddCampaignAsync(CampaignRequestDto dto)
        {
            try
            {
                if (dto.BaslangicTarihi.Date < DateTime.Today || dto.BitisTarihi.Date < DateTime.Today)
                {
                    _logger.LogWarning("Geçmiş tarihli kampanya eklenmeye çalışıldı. Başlangıç: {Start}, Bitiş: {End}", dto.BaslangicTarihi, dto.BitisTarihi);
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Geçmiş tarihli kampanya eklenmeye çalışıldı. Başlangıç: {dto.BaslangicTarihi}, Bitiş: {dto.BitisTarihi}",
                        Source = "CampaignService",
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new ApplicationException("Geçmiş tarihli kampanya eklenemez.");
                }

                var now = DateTime.UtcNow;
                var newCampaign = new Campaign
                {
                    CreditType = (CreditType)dto.CreditType,
                    MinVade = dto.MinVade,
                    MaxVade = dto.MaxVade,
                    MinKrediTutar = dto.MinKrediTutar,
                    MaxKrediTutar = dto.MaxKrediTutar,
                    BaslangicTarihi = dto.BaslangicTarihi,
                    BitisTarihi = dto.BitisTarihi,
                    Description = dto.Description,
                    FaizOrani = dto.FaizOrani,
                    BankId = dto.BankId,
                    RecordTime = DateTime.UtcNow,
                    CampaignStatus = dto.BitisTarihi < now ? CampaignStatus.Expired : CampaignStatus.Active
                };
               

                await _repository.AddAsync(newCampaign);
                _logger.LogInformation("Yeni kampanya eklendi. BankId: {BankId}, Faiz: {Faiz}", dto.BankId, dto.FaizOrani);
                AppMetrics.AppInfoTotal.WithLabels(nameof(CampaignService), "Create").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Yeni kampanya eklendi. BankId: {dto.BankId}, Faiz: {dto.FaizOrani}",
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CampaignService), "Create").Inc();

                _logger.LogError(ex, "Kampanya eklenirken hata oluştu.");
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Kampanya eklenemedi: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("Kampanya eklenemedi", ex);
            }
        }

        public async Task UpdateCampaignAsync(int id, CampaignRequestDto dto)
        {
            try
            {
                if (dto.BaslangicTarihi.Date < DateTime.Today || dto.BitisTarihi.Date < DateTime.Today)
                {
                    _logger.LogWarning("Geçmiş tarihli kampanya güncellenmeye çalışıldı. Başlangıç: {Start}, Bitiş: {End}", dto.BaslangicTarihi, dto.BitisTarihi);
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Geçmiş tarihli kampanya güncellenmeye çalışıldı. Başlangıç: {dto.BaslangicTarihi}, Bitiş: {dto.BitisTarihi}",
                        Source = "CampaignService",
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new ApplicationException("Geçmiş tarihli kampanya güncellenemez.");
                }

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                {
                    _logger.LogWarning("Güncellenmek istenen kampanya bulunamadı. Id: {Id}", id);
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CampaignService), "Update").Inc();

                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Güncellenmek istenen kampanya bulunamadı. Id: {id}",
                        Source = "CampaignService",
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new KeyNotFoundException("Kampanya bulunamadı.");
                }

                existing.CreditType = (CreditType)dto.CreditType;
                existing.MinVade = dto.MinVade;
                existing.MaxVade = dto.MaxVade;
                existing.MinKrediTutar = dto.MinKrediTutar;
                existing.MaxKrediTutar = dto.MaxKrediTutar;
                existing.BaslangicTarihi = dto.BaslangicTarihi;
                existing.BitisTarihi = dto.BitisTarihi;
                existing.Description = dto.Description;
                existing.FaizOrani = dto.FaizOrani;
                existing.BankId = dto.BankId;

                await _repository.UpdateAsync(existing);
                _logger.LogInformation("Kampanya güncellendi. Id: {Id}", id);
                AppMetrics.AppInfoTotal.WithLabels(nameof(CampaignService), "Update").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Kampanya güncellendi. Id: {id}",
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kampanya güncellenirken hata oluştu.");
                AppMetrics.AppErrorTotal.WithLabels(nameof(CampaignService), "Update").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Kampanya güncellenemedi: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("Kampanya güncellenemedi", ex);
            }
        }

        public async Task DeleteCampaignAsync(int id)
        {
            try
            {
                var campaign = await _repository.GetByIdAsync(id);
                if (campaign == null)
                {
                    _logger.LogWarning("Silinmek istenen kampanya bulunamadı. Id: {Id}", id);
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CampaignService), "Delete").Inc();

                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Silinmek istenen kampanya bulunamadı. Id: {id}",
                        Source = "CampaignService",
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new KeyNotFoundException("Silinecek kampanya bulunamadı.");
                }

                await _repository.DeleteAsync(campaign);
                _logger.LogInformation("Kampanya silindi. Id: {Id}", id);
                AppMetrics.AppInfoTotal.WithLabels(nameof(CampaignService), "Delete").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Kampanya silindi. Id: {id}",
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kampanya silinirken hata oluştu.");
                AppMetrics.AppErrorTotal.WithLabels(nameof(CampaignService), "Delete").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Kampanya silinemedi: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("Kampanya silinemedi", ex);
            }
        }

        public async Task<List<CampaignResponseDto>> GetCampaignsByBankAndTypeAsync(string bankName, int creditType)
        {
            try
            {
                var allCampaigns = await _repository.GetActiveCampaignsWithBanksAsync();

                var filtered = allCampaigns
                    .Where(c =>
                        c.Bank != null &&
                        c.Bank.Name.ToLower() == bankName.ToLower() &&
                        (int)c.CreditType == creditType
                    )
                    .ToList();

                _logger.LogInformation("Filtreli kampanyalar getirildi. Banka: {Bank}, KrediTipi: {CreditType}, Eşleşen: {Count}", bankName, creditType, filtered.Count);
                AppMetrics.AppInfoTotal.WithLabels(nameof(CampaignService), "GetAll").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Filtreli kampanyalar getirildi. Banka: {bankName}, KrediTipi: {creditType}, Eşleşen: {filtered.Count}",
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });

                return filtered.Select(c => new CampaignResponseDto
                {
                    Id = c.Id,
                    CreditType = c.CreditType.ToString(),
                    MinVade = c.MinVade,
                    MaxVade = c.MaxVade,
                    MinKrediTutar = c.MinKrediTutar,
                    MaxKrediTutar = c.MaxKrediTutar,
                    BaslangicTarihi = c.BaslangicTarihi,
                    BitisTarihi = c.BitisTarihi,
                    Description = c.Description,
                    FaizOrani = c.FaizOrani,
                    BankId = c.BankId,
                    BankName = c.Bank?.Name ?? "Unknown"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kampanyalar filtrelenirken hata oluştu.");
                AppMetrics.AppErrorTotal.WithLabels(nameof(CampaignService), "GetActive").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Filtreli kampanyalar getirilemedi: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CampaignService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("Filtreli kampanyalar getirilemedi", ex);
            }
        }

        public async Task UpdateExpiredCampaignStatusesAsync()
        {
            var campaigns = await _repository.GetAllAsync();
            var now = DateTime.UtcNow;

            foreach (var campaign in campaigns)
            {
                if (campaign.BitisTarihi < now && campaign.CampaignStatus != CampaignStatus.Expired)
                {
                    campaign.CampaignStatus = CampaignStatus.Expired;
                    await _repository.UpdateAsync(campaign);

                    _logger.LogInformation("Kampanya süresi dolduğu için Expired yapıldı. Id: {Id}", campaign.Id);
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CampaignService), "Update").Inc();
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Info",
                        Message = $"Kampanya Expired yapıldı. Id: {campaign.Id}",
                        Source = "CampaignService",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

    }
}
