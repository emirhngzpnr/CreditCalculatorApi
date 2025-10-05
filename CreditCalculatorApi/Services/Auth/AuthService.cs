using System.Security.Claims;
using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Monitoring;
using CreditCalculatorApi.Repository.Interfaces;
using CreditCalculatorApi.Security;
using CreditCalculatorApi.Security.Interfaces;
using CreditCalculatorApi.Services.Interfaces;

namespace CreditCalculatorApi.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ILogService _logService;
        private readonly ILogger<AuthService> _logger;
        private readonly byte[] _masterKey;

        public AuthService(IUserRepository userRepo, IJwtService jwtService, IConfiguration configuration, IEmailService emailservice, ILogService logService, ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _emailService = emailservice;
           
            var base64Key = configuration["Encryption:MasterKey"];
            if (string.IsNullOrEmpty(base64Key))
                throw new Exception("MasterKey bulunamadı (appsettings.json).");

            _masterKey = Convert.FromBase64String(base64Key);
            _logService = logService;
            _logger = logger;
        }


        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            _logger.LogInformation("Login denemesi başlatıldı | Email={Email}", dto.Email);
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user == null)
            { 
                _logger.LogWarning("Giriş başarısız - kullanıcı bulunamadı | Email={Email}", dto.Email);
                AppMetrics.AppWarningTotal.WithLabels(nameof(AuthService), "Login").Inc();
                await _logService.LogAsync(new LogModel
                {
                    LogType = "LoginFailed",
                    Message = $"Giriş reddedildi - kullanıcı bulunamadı: {dto.Email}",
                    Source = nameof(AuthService),
                    CreatedAt = DateTime.UtcNow
                });
                throw new Exception("Kullanıcı bulunamadı.");
            }

            var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordCorrect)
            {
                _logger.LogWarning("Login reddedildi: şifre hatalı | UserId={UserId} Email={Email}", user.Id, user.Email);
                AppMetrics.AppWarningTotal.WithLabels(nameof(AuthService), "Login").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "LoginFailed",
                    Message = $"Giriş reddedildi - şifre hatalı: {dto.Email}",
                    UserId = user.Id.ToString(),
                    Source = nameof(AuthService),
                    CreatedAt = DateTime.UtcNow
                });
                throw new Exception("Şifre hatalı.");
            }

            if (!user.IsEmailConfirmed)
            {
                _logger.LogWarning("Login engellendi: e-posta doğrulanmamış | UserId={UserId} Email={Email}", user.Id, user.Email);
                AppMetrics.AppWarningTotal.WithLabels(nameof(AuthService), "Login").Inc();
                await _logService.LogAsync(new LogModel
                {
                    LogType = "LoginBlocked",
                    Message = $"E-posta doğrulanmamış kullanıcı giriş yapmaya çalıştı: {user.Email}",
                    UserId = user.Id.ToString(),
                    Source = nameof(AuthService),
                    CreatedAt = DateTime.UtcNow
                });
                throw new Exception("E-posta adresiniz doğrulanmamış. Lütfen e-posta kutunuzu kontrol edin.");
            }
            AppMetrics.UserLoginTotal
        .WithLabels("password")     
        .Inc();
            _logger.LogInformation("Login başarılı | UserId={UserId} Email={Email}", user.Id, user.Email);
            await _logService.LogAsync(new LogModel
            {
                LogType = "LoginSuccess",
                Message = $"Giriş başarılı: {user.Email}",
                UserId = user.Id.ToString(),
                Source = nameof(AuthService),
                CreatedAt = DateTime.UtcNow
            });
            var token = _jwtService.GenerateToken(user);
            AppMetrics.AppInfoTotal.WithLabels(nameof(AuthService), "Login").Inc();

            return new AuthResponseDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

        }
        public async Task SendResetPasswordEmailAsync(string email)
        {
            _logger.LogInformation("Şifre sıfırlama e-postası talebi | Email={Email}", email);
            var user = await _userRepo.GetByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Şifre sıfırlama: kullanıcı bulunamadı | Email={Email}", email);
                AppMetrics.AppWarningTotal.WithLabels(nameof(AuthService), "SendResetEmail").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "PasswordResetFailed",
                    Message = $"Şifre sıfırlama isteği başarısız - kullanıcı bulunamadı: {email}",
                    Source = nameof(AuthService),
                    CreatedAt = DateTime.UtcNow
                });
                throw new ArgumentException("Bu email ile kayıtlı kullanıcı bulunmamaktadır.");
            }

            try
            {
                var token = _jwtService.GenerateResetPasswordToken(user);
                var resetLink = $"http://localhost:4200/reset-password?token={token}";

                await _emailService.SendEmailAsync(email, "Şifre Sıfırlama", $@"
                  Aşağıdaki bağlantıya tıklayarak şifrenizi sıfırlayabilirsiniz:
                 <br><br>
                 <a href='{resetLink}'>Şifreyi Sıfırla</a>");

                _logger.LogInformation("Şifre sıfırlama e-postası gönderildi | UserId={UserId} Email={Email}", user.Id, email);
                AppMetrics.AppInfoTotal.WithLabels(nameof(AuthService), "SendResetEmail").Inc();
                await _logService.LogAsync(new LogModel { /* mevcut bloğun aynı */ });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlama e-postası gönderilemedi | Email={Email}", email);
                AppMetrics.AppErrorTotal.WithLabels(nameof(AuthService), "SendResetEmail").Inc();

                await _logService.LogAsync(new LogModel
                {
                    LogType = "PasswordResetEmailFailed",
                    Message = $"Şifre sıfırlama maili gönderilemedi: {ex.Message} ({email})",
                    Source = nameof(AuthService),
                    CreatedAt = DateTime.UtcNow,
                    Exception = ex.ToString()
                });

                throw;
            }
        }


        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            _logger.LogInformation("Şifre sıfırlama işlemi başlatıldı");

            try
            {
                var principal = _jwtService.ValidateToken(token);
                if (principal == null)
                {
                    _logger.LogWarning("Şifre sıfırlama: token geçersiz veya süresi dolmuş");
                    AppMetrics.AppWarningTotal.WithLabels(nameof(AuthService), "ResetPassword").Inc();

                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "ResetPasswordFailed",
                        Message = "Şifre sıfırlama işlemi başarısız - token geçersiz veya süresi dolmuş.",
                        Source = nameof(AuthService),
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new Exception("Geçersiz veya süresi dolmuş token.");
                }

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Şifre sıfırlama: token geçerli fakat e-posta claim yok");
                    AppMetrics.AppWarningTotal.WithLabels(nameof(AuthService), "ResetPassword").Inc();
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "ResetPasswordFailed",
                        Message = "Şifre sıfırlama işlemi başarısız - token geçerli ama e-posta alınamadı.",
                        Source = nameof(AuthService),
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new Exception("Token geçerli ancak email bulunamadı.");
                }

                var user = await _userRepo.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Şifre sıfırlama: kullanıcı bulunamadı | Email={Email}", email);
                    AppMetrics.AppWarningTotal.WithLabels(nameof(AuthService), "ResetPassword").Inc();
                    await _logService.LogAsync(new LogModel
                    {
                        LogType = "ResetPasswordFailed",
                        Message = $"Şifre sıfırlama işlemi başarısız - kullanıcı bulunamadı: {email}",
                        Source = nameof(AuthService),
                        CreatedAt = DateTime.UtcNow
                    });
                    throw new Exception("Kullanıcı bulunamadı.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _userRepo.UpdateAsync(user);
                _logger.LogInformation("Şifre başarıyla sıfırlandı | UserId={UserId} Email={Email}", user.Id, email);
                await _logService.LogAsync(new LogModel
                {
                    LogType = "PasswordResetSuccess",
                    Message = $"Şifre başarıyla sıfırlandı: {email}",
                    UserId = user.Id.ToString(),
                    Source = nameof(AuthService),
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlama işlemi hata aldı");
                AppMetrics.AppErrorTotal.WithLabels(nameof(AuthService), "ResetPassword").Inc();
                await _logService.LogAsync(new LogModel
                {
                    LogType = "UnhandledException",
                    Message = $"[ResetPasswordError] {ex.Message}",
                    Source = nameof(AuthService),
                    CreatedAt = DateTime.UtcNow,
                    Exception = ex.ToString()
                });
                throw;
            }
        }   


    }

}

