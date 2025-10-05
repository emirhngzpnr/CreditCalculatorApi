using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Entities.Enums;
using CreditCalculatorApi.Monitoring;
using CreditCalculatorApi.Repository.Interfaces;
using CreditCalculatorApi.Security;
using CreditCalculatorApi.Security.Interfaces;
using CreditCalculatorApi.Services.Interfaces;

namespace CreditCalculatorApi.Services.Account
{
    public class AccountService:IAccountService
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogService _logService;
        private readonly ILogger<AccountService> _logger;
        public AccountService(IUserRepository userRepo,IJwtService jwtService,IEmailService emailService, ILogService logService, ILogger<AccountService> logger)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _emailService = emailService;
            _logService = logService;
            _logger = logger;
        }

        public async Task RegisterAsync(RegisterRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Kayıt işlemi başlatıldı | Email={Email}", dto.Email);
                var existing = await _userRepo.GetByEmailAsync(dto.Email);
                if (existing != null)
                {
                    _logger.LogWarning("Bu e-mail adresi zaten kayıtlı | Email={Email}", dto.Email);
                    AppMetrics.AppWarningTotal.WithLabels(nameof(AccountService), "Register").Inc();
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "DuplicateEmail",
                        Message = $"Kayıt reddedildi. E-posta zaten mevcut: {dto.Email}",
                        Source = nameof(AccountService),
                        CreatedAt = DateTime.UtcNow
                    });

                    throw new Exception("Bu e-mail adresi zaten kayıtlı.");
                }

               
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                var (encryptedKey, encryptedIv) = AesService.GenerateEncryptedAesKey();
                string encryptedTc = AesService.EncryptWithMasterKey(dto.IdentityNumber);

                var user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    BirthDate = dto.BirthDate,
                    IdentityNumberEncrypted = encryptedTc,
                    PasswordHash = passwordHash,
                    Role = UserRole.User,
                    UserNumber = await GenerateUniqueUserNumberAsync(),
                    AesKeyEncrypted = encryptedKey,
                    AesIVEncrypted = encryptedIv
                };

                await _userRepo.AddAsync(user);
               
                AppMetrics.UserRegisteredTotal
                .WithLabels("web") 
                 .Inc();
                AppMetrics.AppInfoTotal.WithLabels(nameof(AccountService), "Register").Inc();


                await _logService.LogAsync(new LogModel
                {
                    LogType = "UserRegister",
                    Message = $"Yeni kullanıcı kaydı: {user.FirstName} {user.LastName} ({user.Email})",
                    UserId = user.Id.ToString(),
                    Source = nameof(AccountService),
                    CreatedAt = DateTime.UtcNow
                });

                try
                {
                    var token = _jwtService.GenerateEmailConfirmationToken(user);
                    var confirmationLink = $"http://localhost:4200/confirm-email?token={token}";

                    await _emailService.SendEmailAsync(user.Email, "E-posta Doğrulama",
                        $"Merhaba {user.FirstName},<br><br>" +
                        $"Hesabını aktifleştirmek için aşağıdaki bağlantıya tıklayınız:<br>" +
                        $"<a href='{confirmationLink}'>E-posta Adresini Doğrula</a><br><br>" +
                        $"Bu bağlantı 15 dakika geçerlidir.");
                    _logger.LogInformation("Doğrulama e-postası gönderildi | UserId={UserId} Email={Email}", user.Id, user.Email);
                    AppMetrics.AppInfoTotal.WithLabels(nameof(AccountService), "SendConfirmEmail").Inc();

                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Doğrulama e-postası gönderilemedi | UserId={UserId} Email={Email}", user.Id, user.Email);
                    AppMetrics.AppErrorTotal.WithLabels(nameof(AccountService), "Register").Inc();

                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "EmailSendFailed",
                        Message = $"Doğrulama maili gönderilemedi: {emailEx.Message} ({user.Email})",
                        UserId = user.Id.ToString(),
                        Source = nameof(AccountService),
                        CreatedAt = DateTime.UtcNow
                    });
                

                    throw new Exception("Kayıt başarılı ancak doğrulama maili gönderilemedi.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt işlemi hata aldı | Email={Email}", dto.Email);
                await _logService.LogAsync(new LogModel
                {

                    LogType = "UnhandledException",
                    Message = $"[RegisterError] {ex.Message}",
                    Exception = ex.ToString(),
                    Source = nameof(AccountService),
                    CreatedAt = DateTime.UtcNow
                });

               
                if (ex.Message.Contains("zaten kayıtlı"))
                    throw;

                throw new Exception("Kayıt işlemi sırasında bir hata oluştu.");
            }

        }

        private async Task<string> GenerateUniqueUserNumberAsync()
        {
            var random = new Random();
            string userNumber;

            do
            {
                string randomPart = random.Next(1000000000, int.MaxValue).ToString().Substring(0, 10);
                userNumber = "10" + randomPart;
            }
            while (await _userRepo.AnyByUserNumberAsync(userNumber));

            return userNumber;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }

        public async Task ConfirmEmailAsync(string token)
        {
            _logger.LogInformation("E-posta doğrulama başlatıldı");

            var principal = _jwtService.GetPrincipalFromToken(token);
            if (principal == null)
            {
                _logger.LogWarning("E-posta doğrulama başarısız: geçersiz token");
                AppMetrics.AppWarningTotal.WithLabels(nameof(AccountService), "ConfirmEmail").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Warning",
                    Message = "E-posta doğrulama başarısız: geçersiz token",
                    Source = nameof(AccountService),
                    CreatedAt = DateTime.UtcNow
                });

                throw new Exception("Geçersiz token");
            }

            var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("E-posta doğrulama başarısız: token içinde e-posta claim yok");
                AppMetrics.AppWarningTotal.WithLabels(nameof(AccountService), "ConfirmEmail").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Warning",
                    Message = "E-posta doğrulama başarısız: token geçerli ama e-posta claim yok",
                    Source = nameof(AccountService),
                    CreatedAt = DateTime.UtcNow
                });

                throw new Exception("Token içinde e-posta bulunamadı");
            }

            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("E-posta doğrulama başarısız: kullanıcı bulunamadı | Email={Email}", email);
                AppMetrics.AppWarningTotal.WithLabels(nameof(AccountService), "ConfirmEmail").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "Warning",
                    Message = $"E-posta doğrulama başarısız: kullanıcı bulunamadı ({email})",
                    Source = nameof(AccountService),
                    CreatedAt = DateTime.UtcNow
                });

                throw new Exception("Kullanıcı bulunamadı");
            }

            user.IsEmailConfirmed = true;
            await _userRepo.UpdateAsync(user);

            _logger.LogInformation("E-posta doğrulandı | UserId={UserId} Email={Email}", user.Id, user.Email);
            AppMetrics.AppInfoTotal.WithLabels(nameof(AccountService), "ConfirmEmail").Inc();

            await _logService.LogAsync(new LogModel
            {
                LogType = "Info",
                Message = $"E-posta doğrulandı: {user.Email}",
                UserId = user.Id.ToString(),
                Source = nameof(AccountService),
                CreatedAt = DateTime.UtcNow
            });
        }


    }
}
