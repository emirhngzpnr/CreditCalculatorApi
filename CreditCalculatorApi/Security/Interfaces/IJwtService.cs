using System.Security.Claims;
using CreditCalculatorApi.Entities;

namespace CreditCalculatorApi.Security.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateResetPasswordToken(User user);
        ClaimsPrincipal ValidateToken(string token);
        string GenerateEmailConfirmationToken(User user);
        ClaimsPrincipal? GetPrincipalFromToken(string token);

    }
}
