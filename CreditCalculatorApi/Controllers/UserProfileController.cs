using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Services.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditCalculatorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController:ControllerBase
    {
        private readonly IUserProfileService _profileService;

        public UserProfileController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Giriş yapan kullanıcının profil bilgilerini getirir.
        /// </summary>
        /// <returns>Kullanıcının profil bilgileri</returns>
        /// <response code="200">Profil başarıyla getirildi</response>
        /// <response code="401">Kullanıcı doğrulanamadı</response>
        [HttpGet("current")]
        [ProducesResponseType(typeof(UserProfileResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserProfileResponseDto>> GetCurrentUserProfile()
        {
            var result = await _profileService.GetCurrentUserProfileAsync();
            return Ok(result);
        }

        /// <summary>
        /// Giriş yapan kullanıcının profil bilgilerini günceller.
        /// </summary>
        /// <param name="dto">Yeni profil bilgileri</param>
        /// <returns>Başarılı ise 204 No Content</returns>
        /// <response code="204">Profil başarıyla güncellendi</response>
        /// <response code="400">Geçersiz veri gönderildi</response>
        /// <response code="401">Kullanıcı doğrulanamadı</response>
        [HttpPut]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileRequestDto dto)
        {
            await _profileService.UpdateUserProfileAsync(dto);
            return NoContent(); // Başarılı ama içerik dönülmüyor
        }
    }
}
