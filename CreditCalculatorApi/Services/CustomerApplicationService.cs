using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Exceptions;
using CreditCalculatorApi.Repository.Interfaces;
using CreditCalculatorApi.Services.Interfaces;
using CreditCalculatorApi.Services.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CreditCalculatorApi.Monitoring; 


namespace CreditCalculatorApi.Services
{
    public class CustomerApplicationService:ICustomerApplicationService
    {
        private readonly ICustomerApplicationRepository _repository;
        private readonly ILogger<CustomerApplicationService> _logger;
   
        private readonly IBankService _bankService;  
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogService _logService;
        private readonly INotificationService _notificationService;

        public CustomerApplicationService(ICustomerApplicationRepository repository, ILogger<CustomerApplicationService> logger,IBankService bankService, IHttpContextAccessor httpContextAccessor, ILogService logService, INotificationService notificationService)
        {
            _repository = repository;
            _logger = logger;
            
            _bankService = bankService;
            _httpContextAccessor = httpContextAccessor;
            _logService = logService;
            _notificationService = notificationService;
        }

        public async Task<CustomerApplicationResponseDto> CreateAsync(CustomerApplicationRequestDto dto)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
            try
            {


                AppMetrics.AppInfoTotal.WithLabels(nameof(CustomerApplicationService), "CreateAttempt").Inc();

                _logger.LogInformation("Yeni müşteri başvurusu alındı: {Name} {SurName}, TC: {IdentityNumber}", dto.Name, dto.SurName, dto.IdentityNumber);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationStart",
                    UserId = userId,
                    Source = "CustomerApplicationService",
                    Message = $"Yeni müşteri başvurusu alındı: {dto.Name} {dto.SurName}, TC: {dto.IdentityNumber}, Banka: {dto.BankName}"
                });

                
                var existing = await _repository.GetByIdentityAndBankAsync(dto.IdentityNumber, dto.BankName);

                if (existing != null)
                {
                    _logger.LogWarning("Aynı TC ile bu bankaya zaten başvuru yapılmış. TC: {TC}, Banka: {Bank}", dto.IdentityNumber, dto.BankName);
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CustomerApplicationService), "Create").Inc();

                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "CustomerApplicationDuplicate",
                        UserId = userId,
                        Source = "CustomerApplicationService",
                        Message = $"TC {dto.IdentityNumber} ile {dto.BankName} bankasına daha önce başvuru yapılmış."
                    });
                    throw new DuplicateCustomerApplicationException(dto.IdentityNumber, dto.BankName);
                }
                int bankId = await _bankService.GetBankIdByNameAsync(dto.BankName);

                var entity = new CustomerApplication
                {
                    Name = dto.Name,
                    SurName = dto.SurName,
                    IdentityNumber = dto.IdentityNumber,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    BirthDate = dto.BirthDate,
                    BankName = dto.BankName,
                    CustomerNumber = await GenerateCustomerNumberAsync(bankId),
                       UserId = userId ?? string.Empty
                };

                await _repository.AddAsync(entity);

              
                string bankLabel = (entity.BankName ?? "unknown").Trim().ToLowerInvariant();

            
                AppMetrics.CustomerAppsCreatedTotal
                    .WithLabels(bankLabel)
                    .Inc();
                AppMetrics.AppInfoTotal.WithLabels(nameof(CustomerApplicationService), "Create").Inc();

                _logger.LogInformation("Müşteri başvurusu başarıyla kaydedildi. ID: {ApplicationId}, Banka: {BankName}", entity.Id, entity.BankName);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationSuccess",
                    UserId = userId,
                    Source = "CustomerApplicationService",
                    Message = $"Müşteri başvurusu başarıyla kaydedildi. Başvuru ID: {entity.Id}, Banka: {entity.BankName}"
                });
                await _notificationService.SendCustomerApplicationReceivedAsync(entity);



                return new CustomerApplicationResponseDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    SurName = entity.SurName,
                    BankName = entity.BankName,
                    ApplicationDate = entity.ApplicationDate,
                    CustomerNumber=entity.CustomerNumber
                };
            }
            catch (DuplicateCustomerApplicationException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CustomerApplicationService), "Create").Inc();

                _logger.LogError(ex, "Müşteri başvurusu oluşturulurken bir hata oluştu. DTO: {@dto}", dto);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationError",
                    UserId = userId,
                    Source = "CustomerApplicationService",
                    Message = $"Müşteri başvurusu sırasında hata oluştu: {ex.Message}. DTO: {dto.Name} {dto.SurName}, TC: {dto.IdentityNumber}"
                });
                throw new ApplicationException("Müşteri başvurusu sırasında bir hata oluştu.", ex);
            }

        }
        public async Task<List<CustomerApplicationResponseDto>> GetByBankNameAsync(string bankName)
        {
            try
            {
                var applications = await _repository.GetAllAsync();

                var filtered = applications
                    .Where(c => c.BankName.Equals(bankName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                _logger.LogInformation("{Count} adet başvuru listelendi. Banka: {BankName}", filtered.Count, bankName);
                AppMetrics.AppInfoTotal.WithLabels(nameof(CustomerApplicationService), "ListByBank").Inc();


                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationListByBank",
                    UserId = null, 
                    Source = "CustomerApplicationService",
                    Message = $"{bankName} bankası için {filtered.Count} adet müşteri başvurusu listelendi."
                });

                return filtered.Select(c => new CustomerApplicationResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    SurName = c.SurName,
                    BankName = c.BankName,
                    ApplicationDate = c.ApplicationDate,
                    Status = c.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CustomerApplicationService), "ListByBank").Inc();


                _logger.LogError(ex, "{BankName} bankasına ait başvurular listelenirken hata oluştu.", bankName);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationListByBankError",
                    UserId = null,
                    Source = "CustomerApplicationService",
                    Message = $"{bankName} bankasına ait başvurular listelenirken hata oluştu: {ex.Message}"
                });

                throw;
            }
        }

        public async Task UpdateStatusAsync(int applicationId, CustomerApplicationStatus newStatus)
        {
            try
            {
                var application = await _repository.GetByIdAsync(applicationId);

                if (application == null)
                {
                    _logger.LogWarning("Statü güncelleme başarısız: Başvuru bulunamadı. ID: {Id}", applicationId);
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CustomerApplicationService), "UpdateStatus").Inc();

                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "CustomerApplicationStatusUpdateFailed",
                        UserId = null,
                        Source = "CustomerApplicationService",
                        Message = $"Statü güncelleme başarısız. Başvuru bulunamadı. ID: {applicationId}"
                    });

                    throw new Exception("Başvuru bulunamadı.");
                }

                application.Status = newStatus;

                await _repository.SaveChangesAsync();
                AppMetrics.AppInfoTotal.WithLabels(nameof(CustomerApplicationService), "UpdateStatus").Inc();

                string bankLabel = (application.BankName ?? "unknown").Trim().ToLowerInvariant();

                
                if (newStatus == CustomerApplicationStatus.Approved)
                {
                    AppMetrics.CustomerAppsApprovedTotal
                        .WithLabels(bankLabel)  
                        .Inc();
                }
                else if (newStatus == CustomerApplicationStatus.Rejected)
                {
                    AppMetrics.CustomerAppsRejectedTotal
                        .WithLabels(bankLabel)
                        .Inc();
                }

                _logger.LogInformation("Başvuru statüsü güncellendi. ID: {Id}, Yeni Statü: {Status}", applicationId, newStatus);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationStatusUpdated",
                    UserId = null,
                    Source = "CustomerApplicationService",
                    Message = $"Başvuru statüsü güncellendi. ID: {applicationId}, Yeni Statü: {newStatus}"
                });

                await _notificationService.SendCustomerStatusUpdateAsync(application);

            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CustomerApplicationService), "UpdateStatus").Inc();

                _logger.LogError(ex, "Başvuru statüsü güncellenirken hata oluştu. ID: {Id}", applicationId);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationStatusUpdateError",
                    UserId = null,
                    Source = "CustomerApplicationService",
                    Message = $"Başvuru statüsü güncellenirken hata oluştu. ID: {applicationId}, Hata: {ex.Message}"
                });

                throw;
            }
        }


        public async Task DeleteAsync(int id)
        {
            try
            {
                var application = await _repository.GetByIdAsync(id);
                if (application == null)
                {
                    _logger.LogWarning("Silme işlemi başarısız: Başvuru bulunamadı. ID: {Id}", id);
                    AppMetrics.AppWarningTotal.WithLabels(nameof(CustomerApplicationService), "Delete").Inc();

                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "CustomerApplicationDeleteFailed",
                        UserId = null,
                        Source = "CustomerApplicationService",
                        Message = $"Silme işlemi başarısız. Başvuru bulunamadı. ID: {id}"
                    });

                    throw new Exception("Başvuru bulunamadı.");
                }

                await _repository.DeleteAsync(application);
                AppMetrics.AppInfoTotal.WithLabels(nameof(CustomerApplicationService), "Delete").Inc();

                _logger.LogInformation("Başvuru silindi. ID: {Id}", id);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationDeleted",
                    UserId = null,
                    Source = "CustomerApplicationService",
                    Message = $"Başvuru başarıyla silindi. ID: {id}, Banka: {application.BankName}"
                });
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CustomerApplicationService), "Delete").Inc();

                _logger.LogError(ex, "Başvuru silinirken hata oluştu. ID: {Id}", id);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationDeleteError",
                    UserId = null,
                    Source = "CustomerApplicationService",
                    Message = $"Başvuru silinirken hata oluştu. ID: {id}, Hata: {ex.Message}"
                });

                throw;
            }
        }


        public async Task<bool> IsCustomerAsync(string email, string bankName)
        {
            return await _repository.IsCustomerAsync(email, bankName);
        }
        private async Task<string> GenerateCustomerNumberAsync(int bankId)
        {
            string number;
            bool exists;
            var random = new Random();
            int retryCount = 0;

            string prefix = bankId < 10
                ? $"0{bankId}"
                : $"{bankId}";

            do
            {
                var randomPart = random.Next(1000000000, int.MaxValue).ToString().PadLeft(10, '0').Substring(0, 10);
                number = prefix + randomPart;

                exists = await _repository.CustomerNumberExistsAsync(number);
                retryCount++;
            }
            while (exists);

            await _logService.LogAsync(new LogModel
            {
                LogType = "CustomerNumberGenerated",
                UserId = null,
                Source = "CustomerApplicationService",
                Message = $"Yeni müşteri numarası üretildi: {number} (Banka ID: {bankId}, Deneme sayısı: {retryCount})"
            });

            return number;
        }



        public async Task<List<CustomerApplicationResponseDto>> GetByUserIdAsync(string userId)
        {
            try
            {
                var applications = await _repository.GetByUserIdAsync(userId);

                _logger.LogInformation("Kullanıcıya ait başvurular listelendi. UserId: {UserId}, Toplam: {Count}", userId, applications.Count);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationListByUser",
                    UserId = userId,
                    Source = "CustomerApplicationService",
                    Message = $"Kullanıcının ({userId}) başvuruları listelendi. Toplam kayıt: {applications.Count}"
                });
                AppMetrics.AppInfoTotal.WithLabels(nameof(CustomerApplicationService), "ListByUser").Inc();

                return applications.Select(app => new CustomerApplicationResponseDto
                {
                    Id = app.Id,
                    Name = app.Name,
                    SurName = app.SurName,
                    BankName = app.BankName,
                    ApplicationDate = app.ApplicationDate,
                    Status = app.Status,
                    CustomerNumber = app.CustomerNumber
                }).ToList();
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CustomerApplicationService), "ListByUser").Inc();

                _logger.LogError(ex, "Kullanıcının başvuruları çekilirken hata oluştu. UserId: {UserId}", userId);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "CustomerApplicationListByUserError",
                    UserId = userId,
                    Source = "CustomerApplicationService",
                    Message = $"Kullanıcının başvuruları çekilirken hata oluştu: {ex.Message}"
                });

                throw;
            }
        }


        public async Task<List<CustomerApplicationResponseDto>> GetApprovedMembershipsAsync(string userId)
        {
            try
            {
                var apps = await _repository.GetByUserIdAsync(userId);
                var approved = apps
                    .Where(app => app.Status == CustomerApplicationStatus.Approved)
                    .ToList();

                _logger.LogInformation("Kullanıcının onaylı üyelikleri listelendi. UserId: {UserId}, Sayı: {Count}", userId, approved.Count);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "ApprovedMembershipsListed",
                    UserId = userId,
                    Source = "CustomerApplicationService",
                    Message = $"Kullanıcının ({userId}) onaylı üyelikleri listelendi. Toplam: {approved.Count}"
                });
                AppMetrics.AppInfoTotal.WithLabels(nameof(CustomerApplicationService), "GetApprovedMemberships").Inc();

                return approved.Select(app => new CustomerApplicationResponseDto
                {
                    Id = app.Id,
                    Name = app.Name,
                    SurName = app.SurName,
                    BankName = app.BankName,
                    ApplicationDate = app.ApplicationDate,
                    Status = app.Status,
                    CustomerNumber = app.CustomerNumber
                }).ToList();
            }
            catch (Exception ex)
            {
                AppMetrics.AppErrorTotal.WithLabels(nameof(CustomerApplicationService), "GetApprovedMemberships").Inc();

                _logger.LogError(ex, "Onaylı üyelikler getirilirken hata oluştu. UserId: {UserId}", userId);

                await _logService.LogAsync(new LogModel
                {
                    LogType = "ApprovedMembershipsError",
                    UserId = userId,
                    Source = "CustomerApplicationService",
                    Message = $"Onaylı üyelikler getirilirken hata oluştu: {ex.Message}"
                }); 

                throw;
            }
        }







    }

}

