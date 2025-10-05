using System.Security.Claims;
using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Security.Interfaces;
using CreditCalculatorApi.Services.Account;
using CreditCalculatorApi.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;

namespace CreditCalculatorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
     // Kullanıcı kimlik doğrulaması gerektirir
    public class AuthController:ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAccountService _accountService;
        private readonly IJwtService _jwtService;

        public AuthController(IAccountService accountService,IAuthService authService,IJwtService jwtService)
        {
            _accountService = accountService;
            _authService = authService;
            _jwtService = jwtService;


        }

        /// <summary>
        /// Yeni bir kullanıcı kaydı oluşturur.
        /// </summary>
        /// <param name="dto">Kayıt için gerekli bilgiler</param>
        /// <remarks>
        /// E-posta doğrulaması için kullanıcıya bir onay bağlantısı gönderilir.
        /// </remarks>
        /// <returns>Kayıt başarılıysa 200 OK</returns>
        /// <response code="200">Kayıt başarılı</response>
        /// <response code="400">Geçersiz giriş veya kullanıcı zaten mevcut</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            try
            {
                await _accountService.RegisterAsync(dto);
                return Ok(new { message = "Kayıt başarılı" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Kullanıcı girişi yapar ve JWT token döner.
        /// </summary>
        /// <param name="dto">Giriş bilgileri</param>
        /// <returns>JWT token ve kullanıcı bilgileri</returns>
        /// <response code="200">Giriş başarılı</response>
        /// <response code="400">Hatalı e-posta veya şifre</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Şifremi unuttum bağlantısı gönderir.
        /// </summary>
        /// <param name="dto">Kullanıcının e-posta adresi</param>
        /// <returns>Başarılıysa 204 No Content</returns>
        /// <response code="204">E-posta gönderildi</response>
        /// <response code="404">Kullanıcı bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            try
            {
                await _authService.SendResetPasswordEmailAsync(dto.email);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message }); // 404 ve mesaj body içinde
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + ex.Message });
            }
        }


        /// <summary>
        /// Şifre sıfırlama işlemini gerçekleştirir.
        /// </summary>
        /// <param name="dto">Yeni şifre ve token bilgisi</param>
        /// <returns>Başarılıysa 200 OK</returns>
        /// <response code="200">Şifre başarıyla güncellendi</response>
        /// <response code="400">Token geçersiz veya işlem başarısız</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// E-posta adresini doğrular.
        /// </summary>
        /// <param name="token">JWT doğrulama token'ı (query string ile)</param>
        /// <returns>Doğrulama sonucu mesajı</returns>
        /// <response code="200">Doğrulama başarılı</response>
        /// <response code="400">Geçersiz veya süresi dolmuş token</response>
        /// <response code="404">Kullanıcı bulunamadı</response>
        [HttpGet("confirm-email")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var principal = _jwtService.GetPrincipalFromToken(token);
            if (principal == null)
                return BadRequest("Geçersiz veya süresi dolmuş doğrulama linki.");

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var purpose = principal.FindFirst("purpose")?.Value;

            if (email == null || purpose != "email-confirmation")
                return BadRequest("Geçersiz doğrulama token'ı.");

            var user = await _accountService.GetByEmailAsync(email);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            if (user.IsEmailConfirmed)
                return Ok("E-posta zaten doğrulanmış.");

            await _accountService.ConfirmEmailAsync(token);

            return Ok("E-posta başarıyla doğrulandı. Artık giriş yapabilirsiniz.");
        }



    }
}
