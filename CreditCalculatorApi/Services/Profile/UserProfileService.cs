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

        public UserProfileService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserProfileResponseDto> GetCurrentUserProfileAsync()
        {
            var user = await GetAuthenticatedUserAsync();

            //  Şifreli TC logu
            Console.WriteLine(" Şifreli TC (Encrypted): " + user.IdentityNumberEncrypted);

            string decryptedTc = "";
            try
            {
                //  Çözme girişimi
                decryptedTc = AesService.DecryptWithMasterKey(user.IdentityNumberEncrypted);

                //  Başarılıysa açık TC logu
                Console.WriteLine(" Açık TC (Decrypted): " + decryptedTc);
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
