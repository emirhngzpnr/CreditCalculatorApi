using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Services.Interfaces
{
    public interface ICreditService
    {
        CreditResponseDto Hesapla(CreditRequestDto request);
        Task<CreditResponseDto> HesaplaVeKaydetAsync(CreditRequestDto request);
        Task<List<CreditCalculation>> GetAllCalculationsAsync();
        Task<List<CreditCalculation>> GetFilteredCalculationsAsync(decimal? minFaiz, decimal? maxFaiz, int? vade);

    }
}
