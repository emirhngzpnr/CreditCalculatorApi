using CreditCalculatorApi.Data;
using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.DTOs.Reports;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Monitoring;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreditCalculatorApi.Services
{
    public class BankService:IBankService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BankService> _logger;
        private readonly ILogService _logService;

        public BankService(ApplicationDbContext context, ILogger<BankService> logger, ILogService logService)
        {
            _context = context;
            _logger = logger;
            _logService = logService;
        }

        public async Task<List<BankResponseDto>> GetAllBanksAsync()
        {
            try
            {
                var banks = await _context.Banks
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Toplam {BankCount} banka bulundu.", banks.Count);
              
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Toplam {banks.Count} banka listelendi.",
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });

                foreach (var bank in banks)
                {
                    _logger.LogDebug("Banka: Id={Id}, Name={Name}", bank.Id, bank.Name);
                }
               
                AppMetrics.AppInfoTotal.WithLabels(nameof(BankService), "GetAll").Inc();

                return banks.Select(b => new BankResponseDto
                {
                    Id = b.Id,
                    Name = b.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(BankService), "<Action>").Inc();
                _logger.LogError(ex, "Veritabanından bankalar getirilirken bir hata oluştu.");
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Bankaları getirirken hata oluştu: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });
               
                throw new ApplicationException("Bank verileri alınırken bir hata oluştu.", ex);
            }
        }

        public async Task<BankResponseDto> AddBankAsync(BankRequestDto dto)
        {
            try
            {
                var bank = new Bank { Name = dto.Name };
                _context.Banks.Add(bank);
                await _context.SaveChangesAsync();
                AppMetrics.AppInfoTotal.WithLabels(nameof(BankService), "Create").Inc();

                _logger.LogInformation("Yeni banka eklendi. Id={Id}, Name={Name}", bank.Id, bank.Name);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Yeni banka eklendi. Id={bank.Id}, Name={bank.Name}",
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });

                return new BankResponseDto { Id = bank.Id, Name = bank.Name };
            }
            catch (Exception ex)

            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(BankService), "<Action>").Inc();
                _logger.LogError(ex, "Yeni banka eklenirken hata oluştu.");
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Yeni banka eklenirken hata oluştu: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("Banka ekleme sırasında hata oluştu.", ex);
            }
        }

        public async Task UpdateBankAsync(int id, BankRequestDto dto)
        {
            try
            {
                var bank = await _context.Banks.FindAsync(id);
                if (bank == null)
                {
                    AppMetrics.AppWarningTotal.WithLabels(nameof(BankService), "Update").Inc();
                    _logger.LogWarning("Güncellenecek banka bulunamadı. Id={Id}", id);
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Güncellenecek banka bulunamadı. Id={id}",
                        Source = "BankService",
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new KeyNotFoundException("Banka bulunamadı.");
                }

                bank.Name = dto.Name;
                await _context.SaveChangesAsync();
                AppMetrics.AppInfoTotal.WithLabels(nameof(BankService), "Update").Inc();
                _logger.LogInformation("Banka güncellendi. Id={Id}, YeniAd={Name}", id, dto.Name);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Banka güncellendi. Id={id}, YeniAd={dto.Name}",
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(BankService), "<Action>").Inc();
                _logger.LogError(ex, "Banka güncellenirken hata oluştu. Id={Id}", id);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Banka güncellenirken hata oluştu: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("Banka güncellenemedi.", ex);
            }
        }

        public async Task DeleteBankAsync(int id)
        {
            try
            {
                var bank = await _context.Banks.FindAsync(id);
                if (bank == null)
                {
                    AppMetrics.AppWarningTotal.WithLabels(nameof(BankService), "Delete").Inc();
                    _logger.LogWarning("Silinecek banka bulunamadı. Id={Id}", id);
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Silinecek banka bulunamadı. Id={id}",
                        Source = "BankService",
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new KeyNotFoundException("Banka bulunamadı.");
                }

                _context.Banks.Remove(bank);
                await _context.SaveChangesAsync();
                AppMetrics.AppInfoTotal.WithLabels(nameof(BankService), "Delete").Inc();
                _logger.LogInformation("Banka silindi. Id={Id}, Name={Name}", bank.Id, bank.Name);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Banka silindi. Id={bank.Id}, Name={bank.Name}",
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(BankService), "<Action>").Inc();
                _logger.LogError(ex, "Banka silinirken hata oluştu. Id={Id}", id);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Banka silinirken hata oluştu: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("Banka silinemedi.", ex);
            }
        }

        public async Task<int> GetBankIdByNameAsync(string bankName)
        {
            var bank = await _context.Banks
                .FirstOrDefaultAsync(b => b.Name.ToLower() == bankName.ToLower());

            if (bank == null)
            {
                AppMetrics.AppWarningTotal.WithLabels(nameof(BankService), "GetByName").Inc();
                _logger.LogWarning("Banka bulunamadı: {BankName}", bankName);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Warning",
                    Message = $"Banka bulunamadı: {bankName}",
                    Source = "BankService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new Exception($"Banka bulunamadı: {bankName}");
            }

            return bank.Id;
        }
       

    }
}
