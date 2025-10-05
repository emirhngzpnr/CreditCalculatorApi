using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.DTOs.Reports;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditCalculatorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BanksController:ControllerBase
    {
        private readonly IBankService _bankService;
      

        public BanksController(IBankService bankService)
        {
            _bankService = bankService;
          
        }
        /// <summary>
        /// Get all banks
        /// </summary>
        /// <remarks>
        /// Returns a list of all available banks from the system.
        /// </remarks>
        /// <response code="200">Returns the list of banks</response>
        /// <response code="500">If there is an internal server error</response>


        [HttpGet]
        public async Task<ActionResult<List<BankResponseDto>>> GetBanks()
        {
            
                var banks = await _bankService.GetAllBanksAsync();
                return Ok(banks);
            
           
        }
        /// <summary>
        /// Add a new bank
        /// </summary>
        /// <param name="bank">Bank object to be added</param>
        /// <remarks>
        /// Creates a new bank record in the system.
        /// </remarks>
        /// <response code="201">Bank successfully created</response>
        /// <response code="400">If the request data is invalid</response>
        /// <response code="500">If there is an internal server error</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<BankResponseDto>> AddBank([FromBody] BankRequestDto dto)
        {
            
                var created = await _bankService.AddBankAsync(dto);
                return CreatedAtAction(nameof(GetBanks), new { id = created.Id }, created);
            
        }
        /// <summary>
        /// Update an existing bank
        /// </summary>
        /// <param name="id">ID of the bank to update</param>
        /// <param name="bank">Updated bank object</param>
        /// <remarks>
        /// Updates the information of an existing bank by ID.
        /// </remarks>
        /// <response code="204">Bank successfully updated</response>
        /// <response code="400">If the ID in URL and request body do not match</response>
        /// <response code="500">If there is an internal server error</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBank(int id, [FromBody] BankRequestDto dto)
        {
            
                await _bankService.UpdateBankAsync(id, dto);
                return NoContent();
            

           
        }


        /// <summary>
        /// Delete a bank by ID
        /// </summary>
        /// <param name="id">ID of the bank to delete</param>
        /// <remarks>
        /// Removes a bank from the system using its ID.
        /// </remarks>
        /// <response code="204">Bank successfully deleted</response>
        /// <response code="404">If the bank with specified ID is not found</response>
        /// <response code="500">If there is an internal server error</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBank(int id)
        {
           
                await _bankService.DeleteBankAsync(id);
                return NoContent();
           
        }

       
    }
}
