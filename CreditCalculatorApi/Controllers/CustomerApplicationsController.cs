using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Exceptions;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CreditCalculatorApi.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize] // Kullanıcı kimlik doğrulaması gerektirir
    public class CustomerApplicationsController:ControllerBase
    {
        private readonly ICustomerApplicationService _service;
        public CustomerApplicationsController(ICustomerApplicationService service)
        {
            _service = service;
        }
        /// <summary>
        /// Yeni bir müşteri başvurusu oluşturur.
        /// </summary>
        /// <param name="request">Başvuru bilgileri</param>
        /// <returns>Oluşturulan başvuru bilgisi</returns>
        /// <response code="200">Başvuru başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz veri veya daha önce başvuru yapılmış</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("apply")]
        [ProducesResponseType(typeof(CustomerApplicationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Apply([FromBody] CustomerApplicationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(result);
            }
            catch (DuplicateCustomerApplicationException ex)
            {
               
                return BadRequest(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            catch (Exception ex)
            {
            
                return StatusCode(500, new { message = "Beklenmeyen bir hata oluştu." });
            }
        }
        /// <summary>
        /// Belirli bir bankaya yapılan müşteri başvurularını getirir.
        /// </summary>
        /// <param name="bankName">Banka adı</param>
        /// <returns>Banka adına göre başvurular</returns>
        /// <response code="200">Başvurular listelendi</response>
        [HttpGet("by-bank/{bankName}")]
        [ProducesResponseType(typeof(List<CustomerApplicationResponseDto>), 200)]
        public async Task<IActionResult> GetByBank(string bankName)
        {
            var result = await _service.GetByBankNameAsync(bankName);
            return Ok(result);
        }
        /// <summary>
        /// Belirli bir başvurunun durumunu günceller.
        /// </summary>
        /// <param name="id">Başvuru ID</param>
        /// <param name="dto">Yeni durum</param>
        /// <returns>Durum güncellendiyse 204 No Content</returns>
        /// <response code="204">Durum başarıyla güncellendi</response>
        [HttpPut("status/{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] CustomerApplicationStatusDto dto)
        {
            await _service.UpdateStatusAsync(id, dto.Status);
            return NoContent();
        }
        /// <summary>
        /// Belirli bir başvuruyu siler (Sadece Admin).
        /// </summary>
        /// <param name="id">Başvuru ID</param>
        /// <returns>Başvuru silindiyse 204 No Content</returns>
        /// <response code="204">Silme işlemi başarılı</response>
        /// <response code="404">Başvuru bulunamadı</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        /// <summary>
        /// E-posta ve banka adı ile müşterilik kontrolü yapar.
        /// </summary>
        /// <param name="email">Kullanıcı e-postası</param>
        /// <param name="bankName">Banka adı</param>
        /// <returns>True/False</returns>
        /// <response code="200">Sonuç başarıyla döndü</response>
        /// <response code="400">Eksik parametre</response>
        [AllowAnonymous]
        [HttpGet("is-customer")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> IsCustomer([FromQuery] string email, [FromQuery] string bankName)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(bankName))
                return BadRequest(new { message = "Email ve bankName zorunludur." });

            var result = await _service.IsCustomerAsync(email, bankName);
            return Ok(result); // true veya false döner
        }
        /// <summary>
        /// Giriş yapan kullanıcıya ait tüm müşteri başvurularını getirir.
        /// </summary>
        /// <returns>Kullanıcının başvuru listesi</returns>
        /// <response code="200">Liste başarıyla getirildi</response>
        /// <response code="401">Kullanıcı doğrulanamadı</response>
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<CustomerApplicationResponseDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMyApplications()
        {
            // Claim'leri Serilog ile logla
            foreach (var claim in User.Claims)
            {
                Log.Information("[CLAIM] {Type} = {Value}", claim.Type, claim.Value);
            }

            var userId = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                Log.Warning("GetMyApplications çağrısı başarısız: Geçersiz kullanıcı kimliği.");
                return Unauthorized(new { message = "Geçersiz kullanıcı kimliği." });
            }

            Log.Information("GetMyApplications çağrısı alındı. userId: {UserId}", userId);

            var result = await _service.GetByUserIdAsync(userId);

            Log.Information("Kullanıcıya ait {Count} başvuru bulundu.", result.Count);

            return Ok(result);
        }


        /// <summary>
        /// Giriş yapan kullanıcının onaylanmış müşteri üyeliklerini getirir.
        /// </summary>
        /// <returns>Onaylı üyelikler</returns>
        /// <response code="200">Üyelikler başarıyla getirildi</response>
        /// <response code="401">Kullanıcı doğrulanamadı</response>
        [HttpGet("memberships")]
        [ProducesResponseType(typeof(List<CustomerApplicationResponseDto>), 200)]
        [ProducesResponseType(401)]
       
        public async Task<IActionResult> GetApprovedMemberships()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _service.GetApprovedMembershipsAsync(userId);
            return Ok(result);
        }




    }
}
