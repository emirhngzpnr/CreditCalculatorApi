using CreditCalculatorApi.DTOs;
using FluentValidation;

namespace CreditCalculatorApi.Validators
{
    public class BankRequestValidator:AbstractValidator<BankRequestDto>
    {
        public BankRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Bank name is required.")
                .MaximumLength(100).WithMessage("Bank name must not exceed 100 characters.");
        }
    }
}
