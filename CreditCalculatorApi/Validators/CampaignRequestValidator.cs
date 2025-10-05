using CreditCalculatorApi.DTOs;
using FluentValidation;

namespace CreditCalculatorApi.Validators
{
    public class CampaignRequestValidator:AbstractValidator<CampaignRequestDto>
    {
        public CampaignRequestValidator()
        {
            RuleFor(x => x.CreditType)
                .InclusiveBetween(0, 2)
                .WithMessage("Credit type must be between 1 (İhtiyaç), 2 (Taşıt), or 3 (Konut).");

            RuleFor(x => x.MinVade)
                .GreaterThan(0).WithMessage("Minimum vade must be greater than 0.");

            RuleFor(x => x.MaxVade)
                .GreaterThan(0).WithMessage("Maximum vade must be greater than 0.")
                .GreaterThanOrEqualTo(x => x.MinVade)
                .WithMessage("Maximum vade must be greater than or equal to minimum vade.");

            RuleFor(x => x.MinKrediTutar)
                .GreaterThan(0).WithMessage("Minimum credit amount must be greater than 0.");

            RuleFor(x => x.MaxKrediTutar)
                .GreaterThan(0).WithMessage("Maximum credit amount must be greater than 0.")
                .GreaterThanOrEqualTo(x => x.MinKrediTutar)
                .WithMessage("Maximum credit amount must be greater than or equal to minimum credit amount.");

            RuleFor(x => x.BaslangicTarihi)
                .LessThanOrEqualTo(x => x.BitisTarihi)
                .WithMessage("Start date must be before or equal to end date.");

            RuleFor(x => x.BitisTarihi)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("End date must not be in the past.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description must be at most 500 characters.");

            RuleFor(x => x.FaizOrani)
                .GreaterThan(0).WithMessage("Interest rate must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Interest rate must be realistic (<= 100).");

            RuleFor(x => x.BankId)
                .GreaterThan(0).WithMessage("A valid Bank ID is required.");
        }

    }
}
