using CreditCalculatorApi.DTOs;
using FluentValidation;

namespace CreditCalculatorApi.Validators
{
    public class CustomerApplicationRequestValidator:AbstractValidator<CustomerApplicationRequestDto>
    {
        public CustomerApplicationRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("First name is required.")
                .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.");

            RuleFor(x => x.SurName)
                .NotEmpty().WithMessage("Last name is required.")
                .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.");

            RuleFor(x => x.IdentityNumber)
                .NotEmpty().WithMessage("Identity number is required.")
                .Matches(@"^\d{11}$").WithMessage("Identity number must be 11 digits.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^5\d{9}$").WithMessage("Telefon numarası 5 ile başlamalı ve 10 haneli olmalıdır.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Please enter a valid email address.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .Must(BeAtLeast18YearsOld).WithMessage("You must be at least 18 years old.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required.")
                .MaximumLength(100);
        }

        private bool BeAtLeast18YearsOld(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age >= 18;
        }
    }
}
