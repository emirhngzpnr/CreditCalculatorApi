using CreditCalculatorApi.DTOs;
using FluentValidation;

namespace CreditCalculatorApi.Validators
{
    public class CreditApplicationStatusValidator:AbstractValidator<CreditApplicationStatusDto>
    {
        public CreditApplicationStatusValidator()
        {
            RuleFor(x => x.ApplicationId)
                .GreaterThan(0)
                .WithMessage("Geçerli bir başvuru ID girilmelidir.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Geçersiz başvuru durumu girildi.");
        }
    }
}
