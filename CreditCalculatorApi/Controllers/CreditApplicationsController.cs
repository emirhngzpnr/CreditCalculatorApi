using System.Security.Claims;
using CreditCalculatorApi.Data;
using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Exceptions;
using CreditCalculatorApi.Services;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace CreditCalculatorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CreditApplicationsController:ControllerBase
    {
      
        private readonly ICreditApplicationService _creditService;

        public CreditApplicationsController(ICreditApplicationService creditService)
        {
            
            _creditService=creditService;
        }
        /// <summary>
        /// Lists all credit applications.
        /// </summary>
        /// <returns>List of applications</returns>
        /// <response code="200">All applications were successfully retrieved</response>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditApplication>>> GetBasvurular()
        {
            var basvurular = await _creditService.GetAllApplicationsAsync();
            return Ok(basvurular);
        }
        /// <summary>
        /// Creates a new credit application.
        /// </summary>
        /// <remarks>
        /// Accepts application data and creates a new credit application record in the system.
        /// </remarks>
        /// <param name="basvuru">The application data submitted by the user</param>
        /// <returns>The created application</returns>
        /// <response code="201">Application was successfully created</response>
        /// <response code="400">If the request is invalid or a duplicate application is found</response>
        /// <response code="500">An internal server error occurred</response>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(typeof(CreditApplication), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CreditApplication>> PostBasvuru(CreditApplicationRequestDto basvuru)
        {
            
               

                var result = await _creditService.CreateApplicationAsync(basvuru);
                return CreatedAtAction(nameof(GetBasvurular), new { id = result.Id }, result);
            
              

                

        }
        /// <summary>
        /// Updates the status of a credit application.
        /// </summary>
        /// <remarks>
        /// Updates the application status (e.g. Approved, Rejected, Pending) by ID.
        /// </remarks>
        /// <param name="id">The ID of the application to update</param>
        /// <param name="dto">The updated status information</param>
        /// <response code="204">Application status updated successfully</response>
        /// <response code="400">Invalid request or mismatched ID</response>
        /// <response code="404">Application not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("status/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] CreditApplicationStatusDto dto)
        {
            if (dto.ApplicationId != id)
                return BadRequest("ID uyuşmuyor.");

            try
            {
                await _creditService.UpdateStatusAsync(dto.ApplicationId, dto.Status);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Başvuru bulunamadı.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Bir hata oluştu.");
            }
        }

        /// <summary>
        /// Deletes a credit application by ID.
        /// </summary>
        /// <remarks>
        /// Removes the application with the specified ID from the system.
        /// </remarks>
        /// <param name="id">The ID of the application to delete</param>
        /// <response code="204">Application deleted successfully</response>
        /// <response code="404">Application not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            try
            {
                await _creditService.DeleteApplicationAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Başvuru bulunamadı.");
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, "Bir hata oluştu.");
            }
        }
        /// <summary>
        /// Giriş yapan kullanıcıya ait tüm kredi başvurularını listeler.
        /// </summary>
        /// <remarks>
        /// Bu endpoint JWT token üzerinden kullanıcıyı tanır ve ona ait tüm başvuruları döner.
        /// </remarks>
        /// <response code="200">Kullanıcıya ait kredi başvuruları başarıyla listelendi.</response>
        /// <response code="401">Kullanıcı doğrulanamadı veya token geçersiz.</response>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(List<CreditApplicationResponseDto>), 200)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyApplications()
        {
            // Email'i claim'lerden çek (öncelik sırasıyla)
            var email =
                User.FindFirst(ClaimTypes.Email)?.Value ??
                User.FindFirst("email")?.Value ??
                User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var applications = await _creditService.GetByUserEmailAsync(email);
            return Ok(applications);
        }


        /// <summary>
        /// Kullanıcının onaylanan kredi başvurularını döndürür.
        /// </summary>
        /// <returns>Onaylı kredi başvuruları listesi</returns>
        /// <response code="200">Onaylı başvurular başarıyla getirildi</response>
        /// <response code="401">Kullanıcı kimliği çözümlenemedi</response>
        [Authorize]
        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedCredits()
        {
            var userId = User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı kimliği alınamadı.");

            var result = await _creditService.GetApprovedCreditsAsync(userId);
            return Ok(result);
        }


        /// <summary>
        /// Groups applications by credit type and returns the number of applications in each group.
        /// </summary>
        /// <returns>A list containing credit types and the number of applications per type</returns>
        /// <response code="200">Report successfully generated</response>

        [HttpGet("report")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<ActionResult> GetBasvuruRaporu()
        {
            var rapor = await _creditService.GetApplicationReportAsync();
            return Ok(rapor);
        }


    }
}
