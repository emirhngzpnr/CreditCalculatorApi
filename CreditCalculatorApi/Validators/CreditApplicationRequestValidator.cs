using CreditCalculatorApi.DTOs;
using FluentValidation;

namespace CreditCalculatorApi.Validators
{
    public class CreditApplicationRequestValidator:AbstractValidator<CreditApplicationRequestDto>
    {
        public CreditApplicationRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must be at most 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Please enter a valid email address.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
              .Matches(@"^5\d{9}$").WithMessage("Phone number must start with 5 and be 10 digits.");


            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required.")
                .MaximumLength(100);

            RuleFor(x => x.CreditType)
                .IsInEnum().WithMessage("Invalid credit type.");

            RuleFor(x => x.CampaignId)
                .GreaterThan(0).WithMessage("Campaign ID is required and must be greater than 0.");

            RuleFor(x => x.CreditAmount)
                .GreaterThanOrEqualTo(1000).WithMessage("Credit amount must be at least 1,000 TL.")
                .LessThanOrEqualTo(10_000_000).WithMessage("Credit amount must not exceed 10,000,000 TL.");

            RuleFor(x => x.CreditTerm)
                .InclusiveBetween(2, 240).WithMessage("Credit term must be between 2 and 240 months.");

            RuleFor(x => x.MonthlyIncome)
                .GreaterThanOrEqualTo(1000).WithMessage("Monthly income must be at least 1,000 TL.")
                .LessThanOrEqualTo(10_000_000).WithMessage("Monthly income must not exceed 10,000,000 TL.");
        }
    }
}
