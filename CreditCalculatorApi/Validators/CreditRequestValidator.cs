using CreditCalculatorApi.DTOs;
using FluentValidation;

namespace CreditCalculatorApi.Validators
{
    public class CreditRequestValidator:AbstractValidator<CreditRequestDto>
    {
        public CreditRequestValidator()
        {
            RuleFor(x => x.KrediTutari)
                .GreaterThanOrEqualTo(1000).WithMessage("Credit amount must be at least 1,000 TL.")
                .LessThanOrEqualTo(10_000_000).WithMessage("Credit amount must not exceed 10,000,000 TL.");

            RuleFor(x => x.Vade)
                .InclusiveBetween(1, 240).WithMessage("Loan term must be between 1 and 240 months.");

            RuleFor(x => x.FaizOrani)
                .GreaterThanOrEqualTo(0.01m).WithMessage("Interest rate must be at least 0.01%.")
                .LessThanOrEqualTo(100).WithMessage("Interest rate must not exceed 100%.");
        }
    }
}
