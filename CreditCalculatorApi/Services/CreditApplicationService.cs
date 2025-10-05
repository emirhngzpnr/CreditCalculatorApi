using CreditCalculatorApi.Data;
using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Events;
using CreditCalculatorApi.Exceptions;
using CreditCalculatorApi.Helpers;
using CreditCalculatorApi.Messaging;
using CreditCalculatorApi.Messaging.Interfaces;
using CreditCalculatorApi.Services.Interfaces;
using CreditCalculatorApi.Services.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CreditCalculatorApi.Monitoring; 
using System.Diagnostics;

namespace CreditCalculatorApi.Services
{
    public class CreditApplicationService : ICreditApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreditApplicationService> _logger;
        private readonly INotificationService _notificationService;
        private readonly ILogService _logService;
        private readonly IKafkaProducer _producer;
        private readonly KafkaOptions _kafka;

        public CreditApplicationService(ApplicationDbContext context, ILogger<CreditApplicationService> logger, ILogService logService,INotificationService notificationService, IKafkaProducer producer, IOptions<KafkaOptions> kafka
            )
        {
            _context = context;
            _logger = logger;
           _notificationService= notificationService;
            _logService = logService;
            _producer = producer;
            _kafka = kafka.Value;
        }

        public async Task<List<CreditApplication>> GetAllApplicationsAsync()
        {
            try
            {
                var list = await _context.CreditApplications.ToListAsync();
                _logger.LogInformation("Toplam {count} kredi başvurusu bulundu.", list.Count);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Tüm kredi başvuruları listelendi. Toplam: {list.Count}",
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditApplicationService), "GetAll").Inc();

                return list;
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditApplicationService), "GetAll").Inc();

                _logger.LogError(ex, "Kredi başvuruları alınırken bir hata oluştu.");
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Kredi başvuruları alınamadı: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });
                throw new ApplicationException("Başvurular listelenemedi.", ex);
            }
        }

        public async Task<CreditApplication> CreateApplicationAsync(CreditApplicationRequestDto application)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (application == null)
                    throw new ArgumentNullException(nameof(application));

                var existingApplication = await _context.CreditApplications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Email.ToLower() == application.Email.ToLower() && x.BankName.ToLower() == application.BankName.ToLower());

                if (existingApplication != null)
                {
                    AppMetrics.CreditAppsRejectedTotal
                .WithLabels(application.BankName)
                .Inc();
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CreditApplicationService), "Create").Inc();

                    _logger.LogWarning("Bu e-posta ile bu bankaya zaten bir başvuru yapılmış: {Email}, Banka: {Bank}", application.Email, application.BankName);
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Tekrarlanan başvuru. Email: {application.Email}, Banka: {application.BankName}",
                        Source = "CreditApplicationService",
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new DuplicateEmailException();
                }
                var user = await _context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Email.ToLower() == application.Email.ToLower());

                var entity = new CreditApplication
                {
                    FullName = application.FullName,
                    Email = application.Email,
                    PhoneNumber = application.PhoneNumber,
                    BankName = application.BankName,
                    CreditType = application.CreditType,
                    CreditAmount = application.CreditAmount,
                    CreditTerm = application.CreditTerm,
                    CampaignId = application.CampaignId,
                    MonthlyIncome = application.MonthlyIncome,
                    RiskStatus = RiskCalculator.Calculate(application.CreditAmount, application.CreditTerm, application.MonthlyIncome),
                    UserNumber = user?.UserNumber ?? string.Empty,



                };

                _context.CreditApplications.Add(entity);
                await _context.SaveChangesAsync();
                AppMetrics.CreditAppsCreatedTotal
            .WithLabels(entity.BankName, entity.CreditType.ToString())
            .Inc();
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditApplicationService), "Create").Inc();

                var ev = new CreditApplicationCreated
                {
                    ApplicationId = entity.Id,
                    FullName = entity.FullName,
                    Email = entity.Email,
                    PhoneNumber = entity.PhoneNumber,
                    BankName = entity.BankName,
                    CreditType = entity.CreditType,
                    CreditAmount = entity.CreditAmount,
                    CreditTerm = entity.CreditTerm,
                    MonthlyIncome = entity.MonthlyIncome,
                    RiskStatus = entity.RiskStatus,
                    Status = entity.Status.ToString(),
                    RecordTime = entity.RecordTime,
                    CampaignId = entity.CampaignId,
                    UserNumber = user?.UserNumber ?? string.Empty,
               

                };
                // !!
                await _producer.ProduceAsync(_kafka.Topics.CreditApplicationCreated, ev);

                _logger.LogInformation("Başvuru başarıyla kaydedildi. Id={Id}", entity.Id);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Yeni kredi başvurusu alındı. Id={entity.Id}, Banka={entity.BankName}",
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });

                await _notificationService.SendApplicationReceivedAsync(entity);

                return entity;
            }
            catch (DuplicateEmailException dex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditApplicationService), "Create").Inc();

                _logger.LogWarning(dex, "Aynı kullanıcı aynı bankaya başvurdu.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kredi başvurusu oluşturulurken bir hata oluştu.");
                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Kredi başvurusu oluşturulamadı: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });
                if (application != null)
                    AppMetrics.CreditAppsRejectedTotal
                        .WithLabels(application.BankName ?? "unknown")
                        .Inc();
                throw new ApplicationException("Kredi başvurusu oluşturulamadı.", ex);
            }
            finally
            {
               
                AppMetrics.CreditDecisionLatencySeconds
                    .WithLabels("create_pipeline")
                    .Observe(sw.Elapsed.TotalSeconds);
            }
        }

        public async Task<List<object>> GetApplicationReportAsync()
        {
            try
            {
                var rapor = await _context.CreditApplications
                    .GroupBy(c => c.CreditType)
                    .Select(grp => new
                    {
                        CreditType = grp.Key,
                        Count = grp.Count(),
                        AvgAmount = grp.Average(x => x.CreditAmount),
                        AvgTerm = grp.Average(x => x.CreditTerm)
                    })
                    .ToListAsync<object>();

                _logger.LogInformation("Başvuru raporu üretildi. Toplam kredi türü sayısı: {count}", rapor.Count);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Başvuru raporu oluşturuldu. Kredi türü sayısı: {rapor.Count}",
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditApplicationService), "Report").Inc();

                return rapor;
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditApplicationService), "Report").Inc();

                _logger.LogError(ex, "Başvuru raporu oluşturulurken hata oluştu.");

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Başvuru raporu alınamadı: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });

                throw new ApplicationException("Raporlama sırasında hata oluştu.", ex);
            }
        }

        public async Task UpdateStatusAsync(int id, CreditApplicationStatus newStatus)
        {
            try
            {
                var app = await _context.CreditApplications.FindAsync(id);
                if (app == null)
                {
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CreditApplicationService), "UpdateStatus").Inc();

                    _logger.LogWarning("Durumu güncellenecek başvuru bulunamadı. Id={Id}", id);

                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Durumu güncellenecek başvuru bulunamadı. Id={id}",
                        Source = "CreditApplicationService",
                        CreatedAt = DateTime.UtcNow
                    });

                    throw new KeyNotFoundException("Başvuru bulunamadı.");
                }

                app.Status = newStatus;
                await _context.SaveChangesAsync();
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditApplicationService), "UpdateStatus").Inc();

                if (newStatus == CreditApplicationStatus.Approved)
                {
                    AppMetrics.CreditAppsApprovedTotal
                        .WithLabels(app.BankName)
                        .Inc();
                }
                else if (newStatus == CreditApplicationStatus.Rejected)
                {
                    AppMetrics.CreditAppsRejectedTotal
                        .WithLabels(app.BankName)
                        .Inc();
                }

                string durumMetni = newStatus switch
                {
                    CreditApplicationStatus.Approved => "Onaylandı",
                    CreditApplicationStatus.Rejected => "Reddedildi",
                    CreditApplicationStatus.Waiting => "Beklemede",
                    _ => "Bilinmiyor"
                };

                _logger.LogInformation("Başvuru durumu güncellendi. Id: {Id}, Yeni Durum: {DurumMetni}", id, durumMetni);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Başvuru durumu güncellendi. Id: {id}, Yeni Durum: {durumMetni}",
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });

                await _notificationService.SendStatusUpdateAsync(app);
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditApplicationService), "UpdateStatus").Inc();

                _logger.LogError(ex, "Başvuru durumu güncellenirken hata oluştu. Id={Id}", id);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Başvuru durumu güncellenemedi. Id: {id}, Hata: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });

                throw new ApplicationException("Başvuru durumu güncellenemedi.", ex);
            }
        }

        public async Task DeleteApplicationAsync(int id)
        {
            try
            {
                var basvuru = await _context.CreditApplications.FindAsync(id);
                if (basvuru == null)
                {
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CreditApplicationService), "Delete").Inc();

                    _logger.LogWarning("Silinecek başvuru bulunamadı. Id={Id}", id);


                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "Warning",
                        Message = $"Silinecek başvuru bulunamadı. Id: {id}",
                        Source = "CreditApplicationService",
                        CreatedAt = DateTime.UtcNow
                    });

                    throw new KeyNotFoundException("Başvuru bulunamadı.");
                }

                _context.CreditApplications.Remove(basvuru);
                await _context.SaveChangesAsync();
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditApplicationService), "Delete").Inc();
                _logger.LogInformation("Başvuru silindi. ID: {Id}", id);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"Başvuru silindi. ID: {id}",
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditApplicationService), "Delete").Inc();
                _logger.LogError(ex, "Başvuru silinirken hata oluştu. Id={Id}", id);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Başvuru silinemedi. Id: {id}, Hata: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });

                throw new ApplicationException("Başvuru silinemedi.", ex);
            }
        }


        public async Task<List<CreditApplicationResponseDto>> GetByUserEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("E-posta boş olamaz.");

                var applications = await _context.CreditApplications
                    .Include(a => a.Campaign)
                    .Where(a => a.Email.ToLower() == email.ToLower())
                    .ToListAsync();

                _logger.LogInformation("{email} adresine ait {count} başvuru bulundu.", email, applications.Count);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"{email} adresine ait {applications.Count} başvuru listelendi.",
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditApplicationService), "GetByEmail").Inc();

                return applications.Select(a => new CreditApplicationResponseDto
                {
                    Id = a.Id,
                    CreditAmount = a.CreditAmount,
                    CreditTerm = a.CreditTerm,
                    MonthlyIncome = a.MonthlyIncome,
                    RiskStatus = a.RiskStatus,
                    CampaignId = a.CampaignId,
                    CampaignName = a.Campaign?.Name,
                    Status = a.Status,
                    RecordTime = a.RecordTime,
                    BankName=a.BankName,

                }).ToList();
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditApplicationService), "GetByEmail").Inc();

                _logger.LogError(ex, "Kullanıcının başvuruları alınırken hata oluştu. Email={email}", email);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"{email} için başvurular alınamadı: {ex.Message}",
                    Exception = ex.ToString(),
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });

                throw new ApplicationException("Kullanıcının başvuruları alınamadı.", ex);
            }
        }


        public async Task<List<CreditApplicationResponseDto>> GetApprovedCreditsAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(int.Parse(userId));
                if (user == null)
                    throw new Exception("Kullanıcı bulunamadı");

                var approvedApplications = await _context.CreditApplications
                    .Where(app => app.Email == user.Email && app.Status == CreditApplicationStatus.Approved)
                    .ToListAsync();

                var campaignDict = await _context.Campaigns
                    .ToDictionaryAsync(c => c.Id, c => c.Name);

                var result = approvedApplications.Select(app => new CreditApplicationResponseDto
                {
                    Id = app.Id,
                    CreditAmount = app.CreditAmount,
                    CreditTerm = app.CreditTerm,
                    MonthlyIncome = app.MonthlyIncome,
                    CampaignId = app.CampaignId,
                    Status = app.Status,
                    RecordTime = app.RecordTime,
                    BankName = app.BankName,
                    CampaignName = app.Campaign != null
            ? (string.IsNullOrWhiteSpace(app.Campaign.Name)
                ? app.Campaign.Description
                : app.Campaign.Name)
            : null
                }).ToList();

                _logger.LogInformation("{email} kullanıcısının onaylı başvuruları getirildi. Sayı: {count}", user.Email, result.Count);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Info",
                    Message = $"{user.Email} kullanıcısının onaylı başvuruları getirildi. Sayı: {result.Count}",
                    UserId = userId,
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });
                AppMetrics.AppInfoTotal.WithLabels(nameof(CreditApplicationService), "GetApproved").Inc();

                return result;
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CreditApplicationService), "GetApproved").Inc();

                _logger.LogError(ex, "Onaylı krediler alınırken hata oluştu");

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Error",
                    Message = $"Onaylı başvurular alınamadı: {ex.Message}",
                    Exception = ex.ToString(),
                    UserId = userId,
                    Source = "CreditApplicationService",
                    CreatedAt = DateTime.UtcNow
                });

                return new List<CreditApplicationResponseDto>();
            }
        }

    }
}
