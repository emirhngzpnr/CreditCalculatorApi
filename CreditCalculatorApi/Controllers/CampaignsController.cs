using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Services;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditCalculatorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _service;
        private readonly ILogger<CampaignsController> _logger;
        public CampaignsController(ICampaignService service, ILogger<CampaignsController> logger)
        {
            _service = service;
            _logger = logger;
        }
        /// <summary>
        /// Get all campaigns
        /// </summary>
        /// <remarks>
        /// Returns all campaigns from the database.
        /// </remarks>
        /// <response code="200">Returns the list of campaigns</response>
        /// <response code="500">If there is an internal server error</response>

        [HttpGet]
        public async Task<ActionResult<List<CampaignResponseDto>>> GetAllCampaigns()
        {
            try
            {
                var campaigns = await _service.GetAllCampaignsAsync();
                return Ok(campaigns);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Kampanyalar getirilirken hata oluştu.");
                return StatusCode(500, "An error occurred while retrieving campaigns.");
            }

        }
        /// <summary>
        /// Yeni kampanya oluştur.
        /// </summary>
        /// <response code="201">Başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz veri</response>
        /// <response code="500">Sunucu hatası</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCampaign([FromBody] CampaignRequestDto dto)
        {
            try
            {
                await _service.AddCampaignAsync(dto);
                return StatusCode(201); // Created
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kampanya eklenirken hata oluştu.");
                return StatusCode(500, "Kampanya eklenemedi.");
            }
        }
        /// <summary>
        /// Kampanya güncelle.
        /// </summary>
        /// <param name="id">Güncellenecek kampanyanın ID'si</param>
        /// <response code="204">Başarıyla güncellendi</response>
        /// <response code="404">Kampanya bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCampaign(int id, [FromBody] CampaignRequestDto dto)
        {
            try
            {
                await _service.UpdateCampaignAsync(id, dto);
                return NoContent(); // 204
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Kampanya bulunamadı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kampanya güncellenirken hata oluştu.");
                return StatusCode(500, "Kampanya güncellenemedi.");
            }
        }
        /// <summary>
        /// Kampanya sil.
        /// </summary>
        /// <param name="id">Silinecek kampanyanın ID'si</param>
        /// <response code="204">Başarıyla silindi</response>
        /// <response code="404">Kampanya bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            try
            {
                await _service.DeleteCampaignAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Kampanya bulunamadı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kampanya silinirken hata oluştu.");
                return StatusCode(500, "Kampanya silinemedi.");
            }
        }
        /// <summary>
        /// Filters campaigns by bank name and credit type.
        /// </summary>
        /// <remarks>
        /// Returns the list of campaigns that match the given filters.
        /// If no campaigns match, returns an empty array with 200 OK.
        /// </remarks>
        /// <param name="bankName">Bank name to filter (case-insensitive).</param>
        /// <param name="creditType">
        /// Credit type code (e.g. 1=İhtiyaç, 2=Taşıt, 3=Konut).
        /// </param>
        /// <response code="200">List of matching campaigns (can be empty)</response>
        /// <response code="400">Invalid query parameters</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Unexpected server error</response>
        [Authorize]
        [HttpGet("filter")]
        public async Task<ActionResult<List<CampaignResponseDto>>> GetByBankAndCreditType([FromQuery] string bankName, [FromQuery] int creditType)
        {
            try
            {
                var filteredCampaigns = await _service.GetCampaignsByBankAndTypeAsync(bankName, creditType);
                return Ok(filteredCampaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Filtrelenmiş kampanyalar getirilirken hata oluştu.");
                return StatusCode(500, "Kampanyalar getirilirken bir hata oluştu.");
            }
        }

      


    }
}