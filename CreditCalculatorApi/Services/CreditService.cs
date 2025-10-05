using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Monitoring;
using CreditCalculatorApi.Repository.Interfaces;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CreditCalculatorApi.Services
{
    public class CreditService : ICreditService
    {
        private readonly ICreditCalculationRepository _calculationRepo;
        private readonly ILogger<CreditService> _logger;
        private readonly ILogService _logService;

        public CreditService(ICreditCalculationRepository calculationRepo, ILogger<CreditService> logger, ILogService logService)
        {
            _calculationRepo = calculationRepo;
            _logger = logger;
            _logService = logService;
        }

        public CreditResponseDto Hesapla(CreditRequestDto request)
        {
            _logger.LogInformation("Kredi hesaplama başlatıldı: Tutar={Tutar}, Vade={Vade}, FaizOranı={Faiz}",
                request.KrediTutari, request.Vade, request.FaizOrani);

            // INFO (operation = Calculate)
            AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "Calculate").Inc();

            _logService.LogAsync(new LogModel
            {
                LogType = "Info",
                Message = $"Kredi hesaplama başlatıldı: Tutar={request.KrediTutari}, Vade={request.Vade}, Faiz={request.FaizOrani}",
                Source = "CreditService",
                CreatedAt = DateTime.UtcNow
            });

            decimal faizOraniAylik = request.FaizOrani / 100m;
            decimal krediTutari = request.KrediTutari;
            int vade = request.Vade;

            decimal aylikFaiz = faizOraniAylik;
            decimal aylikTaksit = krediTutari * aylikFaiz / (1 - (decimal)Math.Pow((double)(1 + aylikFaiz), -vade));

            var installments = new List<InstallmentDto>();
            decimal kalanAnapara = krediTutari;

            for (int i = 1; i <= vade; i++)
            {
                decimal faiz = kalanAnapara * aylikFaiz;
                decimal anapara = aylikTaksit - faiz;
                kalanAnapara -= anapara;

                installments.Add(new InstallmentDto
                {
                    InstallmentNo = i,
                    Payment = Math.Round(aylikTaksit, 2),
                    Interest = Math.Round(faiz, 2),
                    Principal = Math.Round(anapara, 2),
                    RemainingPrincipal = Math.Round(kalanAnapara > 0 ? kalanAnapara : 0, 2)
                });
            }

            var toplamOdeme = Math.Round(aylikTaksit * vade, 2);
            var aylikTaksitYuvarlanmis = Math.Round(aylikTaksit, 2);

            _logger.LogInformation("Hesaplama tamamlandı: AylıkTaksit={Aylik}, ToplamOdeme={Toplam}",
                aylikTaksitYuvarlanmis, toplamOdeme);

            // INFO (operation = Calculate)
            AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "Calculate").Inc();

            _logService.LogAsync(new LogModel
            {
                LogType = "Info",
                Message = $"Hesaplama tamamlandı: AylıkTaksit={aylikTaksitYuvarlanmis}, ToplamOdeme={toplamOdeme}",
                Source = "CreditService",
                CreatedAt = DateTime.UtcNow
            });

            return new CreditResponseDto
            {
                MonthlyPayment = aylikTaksitYuvarlanmis,
                TotalPayment = toplamOdeme,
                Installments = installments
            };
        }

        public async Task<CreditResponseDto> HesaplaVeKaydetAsync(CreditRequestDto request)
        {
            try
            {
                _logger.LogInformation("HesaplaVeKaydet işlemi başlatıldı.");

              
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "CalculateAndSave").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationStart",
                    UserId = null,
                    Source = "CreditService",
                    Message = $"Anonim kredi hesaplama süreci başlatıldı. Tutar={request.KrediTutari}, Vade={request.Vade}, Faiz={request.FaizOrani}"
                });

                var result = Hesapla(request);

                var calculation = new CreditCalculation
                {
                    KrediTutari = request.KrediTutari,
                    Vade = request.Vade,
                    FaizOrani = request.FaizOrani,
                    AylikTaksit = result.MonthlyPayment,
                    ToplamOdeme = result.TotalPayment,
                    HesaplamaTarihi = DateTime.UtcNow
                };

                _logger.LogInformation("Veritabanına kayıt ediliyor: Tutar={Tutar}, Vade={Vade}, Faiz={Faiz}, AylıkTaksit={Taksit}",
                    calculation.KrediTutari, calculation.Vade, calculation.FaizOrani, calculation.AylikTaksit);

                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "CalculateAndSave").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationSave",
                    UserId = null,
                    Source = "CreditService",
                    Message = $"Anonim hesaplama sonucu kaydediliyor. Tutar={calculation.KrediTutari}, AylıkTaksit={calculation.AylikTaksit}"
                });

                await _calculationRepo.AddAsync(calculation);

                _logger.LogInformation("Kayıt başarıyla tamamlandı.");

              
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "CalculateAndSave").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationSuccess",
                    UserId = null,
                    Source = "CreditService",
                    Message = "Anonim kredi hesaplama ve kayıt başarıyla tamamlandı."
                });

                return result;
            }
            catch (Exception ex)
            {
                // ERROR (operation = CalculateAndSave)
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditService), "CalculateAndSave").Inc();

                _logger.LogError(ex, "HesaplaVeKaydet sırasında hata oluştu.");

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationError",
                    UserId = null,
                    Source = "CreditService",
                    Message = $"Hesaplama sırasında hata oluştu: {ex.Message}",
                    Exception = ex.ToString()
                });

                throw;
            }
        }

        public async Task<List<CreditCalculation>> GetAllCalculationsAsync()
        {
            try
            {
               
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "ListAll").Inc();

                _logger.LogInformation("Tüm kredi hesaplamaları listeleniyor...");

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationListStart",
                    UserId = null,
                    Source = "CreditService",
                    Message = "Tüm kredi hesaplamaları listeleniyor..."
                });

                var result = await _calculationRepo.GetAllAsync();

                _logger.LogInformation("Toplam {Count} kayıt bulundu.", result.Count);

               
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "ListAll").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationListSuccess",
                    UserId = null,
                    Source = "CreditService",
                    Message = $"Toplam {result.Count} kredi hesaplama kaydı bulundu."
                });

                return result;
            }
            catch (Exception ex)
            {
               
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditService), "ListAll").Inc();

                _logger.LogError(ex, "Tüm kredi hesaplamaları getirilirken hata oluştu.");

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationListError",
                    UserId = null,
                    Source = "CreditService",
                    Message = $"Hesaplamalar getirilirken hata oluştu: {ex.Message}",
                    Exception = ex.ToString()
                });

                throw;
            }
        }

        public async Task<List<CreditCalculation>> GetFilteredCalculationsAsync(decimal? minFaiz, decimal? maxFaiz, int? vade)
        {
            try
            {
                _logger.LogInformation("Filtreli hesaplama listesi isteniyor. MinFaiz={Min}, MaxFaiz={Max}, Vade={Vade}",
                    minFaiz, maxFaiz, vade);

            
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "FilterList").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationFilterStart",
                    UserId = null,
                    Source = "CreditService",
                    Message = $"Filtreli hesaplama sorgusu alındı. MinFaiz={minFaiz}, MaxFaiz={maxFaiz}, Vade={vade}"
                });

                var result = await _calculationRepo.FilterAsync(minFaiz, maxFaiz, vade);

                _logger.LogInformation("Filtre sonucu {Count} kayıt bulundu.", result.Count);

              
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditService), "FilterList").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationFilterSuccess",
                    UserId = null,
                    Source = "CreditService",
                    Message = $"Filtre uygulandı. Sonuç: {result.Count} kayıt. MinFaiz={minFaiz}, MaxFaiz={maxFaiz}, Vade={vade}"
                });

                return result;
            }
            catch (Exception ex)
            {
                
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditService), "FilterList").Inc();

                _logger.LogError(ex, "Filtreli kredi hesaplamaları getirilirken hata oluştu.");

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CreditCalculationFilterError",
                    UserId = null,
                    Source = "CreditService",
                    Message = $"Filtreli hesaplamalar getirilirken hata oluştu: {ex.Message}. MinFaiz={minFaiz}, MaxFaiz={maxFaiz}, Vade={vade}",
                    Exception = ex.ToString()
                });

                throw;
            }
        }
    }
}
            