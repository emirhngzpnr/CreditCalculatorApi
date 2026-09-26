using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Repository.Interfaces;
using CreditCalculatorApi.Security;
using System.Security.Claims;

namespace CreditCalculatorApi.Services.Profile
{
    public class UserProfileService:IUserProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AesService _aesService;

        public UserProfileService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor,AesService aesService)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _aesService= aesService;
        }

        public async Task<UserProfileResponseDto> GetCurrentUserProfileAsync()
        {
            var user = await GetAuthenticatedUserAsync();


            string decryptedTc = "";
            try
            {
                //  Çözme girişimi
                decryptedTc = _aesService.DecryptWithMasterKey(user.IdentityNumberEncrypted);

           
            }
            catch (Exception ex)
            {
                //  Hata durumunu logla
                Console.WriteLine(" AES çözme hatası: " + ex.Message);
                decryptedTc = "Çözülemedi";
            }

            return new UserProfileResponseDto
            {
                UserNumber = user.UserNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                IdentityNumber = decryptedTc
            };
        }


        public async Task UpdateUserProfileAsync(UserProfileRequestDto dto)
        {
            var user = await GetAuthenticatedUserAsync();

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;
            

            await _userRepository.UpdateAsync(user); // UpdateAsync metodun varsa kullan
        }

        private async Task<Entities.User> GetAuthenticatedUserAsync()
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                throw new Exception("Kullanıcı doğrulanamadı.");

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new Exception("Kullanıcı bulunamadı.");

            return user;
        }
    }
}
