using CreditCalculatorApi.DTOs;

namespace CreditCalculatorApi.Services.Profile
{
    public interface IUserProfileService
    {
        Task<UserProfileResponseDto> GetCurrentUserProfileAsync();

        Task UpdateUserProfileAsync(UserProfileRequestDto dto);
    }
}
