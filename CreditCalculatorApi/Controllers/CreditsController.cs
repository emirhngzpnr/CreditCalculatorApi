using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Exceptions;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditCalculatorApi.Controllers
{
    [ApiController]
    [Route("api/credits")]
    public class CreditsController : ControllerBase
    {
        private readonly ICreditService _creditService;

        public CreditsController(ICreditService creditService)
        {
            _creditService = creditService;
        }
       
        /// <summary>
        /// Performs credit calculation and saves the result to the database.
        /// </summary>
        /// <param name="request">Credit details</param>
        /// <returns>Saved calculation result</returns>
        /// <response code="200">Calculation successfully saved</response>
        [HttpPost("hesapla-ve-kaydet")]
        public async Task<IActionResult> HesaplaVeKaydet([FromBody] CreditRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var hesaplama = await _creditService.HesaplaVeKaydetAsync(request);
            return Ok(hesaplama);
        }
        /// <summary>
        /// Lists all credit calculations or filters them based on parameters.
        /// </summary>
        /// <param name="minFaiz">Minimum interest rate</param>
        /// <param name="maxFaiz">Maximum interest rate</param>
        /// <param name="vade">Loan term (in months)</param>
        /// <returns>Filtered credit calculation results</returns>
        /// <response code="200">Listing successful</response>

        [Authorize(Roles = "Admin")]
        [HttpGet("rapor")]
        public async Task<IActionResult> Raporla([FromQuery] decimal? minFaiz, [FromQuery] decimal? maxFaiz, [FromQuery] int? vade)
        {
            var result = await _creditService.GetFilteredCalculationsAsync(minFaiz, maxFaiz, vade);
            return Ok(result);
        }



    }

}