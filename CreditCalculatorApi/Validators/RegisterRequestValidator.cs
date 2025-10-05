using CreditCalculatorApi.DTOs;
using FluentValidation;

namespace CreditCalculatorApi.Validators
{
    public class RegisterRequestValidator:AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad boş olamaz.")
                .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad boş olamaz.")
                .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir email giriniz.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Telefon numarası boş olamaz.")
                .Matches(@"^5\d{9}$").WithMessage("Telefon numarası 5 ile başlamalı ve 10 haneli olmalıdır.");

            RuleFor(x => x.IdentityNumber)
                .NotEmpty().WithMessage("T.C. Kimlik Numarası boş olamaz.")
                .Length(11).WithMessage("T.C. Kimlik Numarası 11 haneli olmalıdır.");

            RuleFor(x => x.BirthDate)
    .NotEmpty().WithMessage("Doğum tarihi boş olamaz.")
    .Must(BeAtLeast18YearsOld).WithMessage("18 yaşından küçükler kayıt olamaz.")
    .Must(BeYoungerThan75).WithMessage("75 yaşından büyük kullanıcılar kayıt olamaz.");


            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
                .Matches(@"[A-Z]+").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]+").WithMessage("Şifre en az bir küçük harf içermelidir.")
                .Matches(@"[\+\-%&]+").WithMessage("Şifre en az bir özel karakter (+, -, %, &) içermelidir.");
        }
        private bool BeAtLeast18YearsOld(DateTime birthDate)    
        {
            return birthDate <= DateTime.Today.AddYears(-18);
        }
        private bool BeYoungerThan75(DateTime birthDate)
        {
            return birthDate >= DateTime.Today.AddYears(-75);
        }

    }
}
