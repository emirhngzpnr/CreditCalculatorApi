using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CreditCalculatorApi.Data;
using CreditCalculatorApi.DTOs;
using CreditCalculatorApi.Entities;
using CreditCalculatorApi.Services.Interfaces;

namespace CreditCalculatorApi.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly PdfService _pdfService;
        private readonly ICreditService _creditService;
        private readonly ApplicationDbContext _db;

        public NotificationService(
            IEmailService emailService,
            PdfService pdfService,
            ICreditService creditService,
            ApplicationDbContext db)
        {
            _emailService = emailService;
            _pdfService = pdfService;
            _creditService = creditService;
            _db = db;
        }


        public async Task SendApplicationReceivedAsync(CreditApplication app)
        {
            if (!app.CampaignId.HasValue)
                throw new InvalidOperationException("CampaignId yok. Bu akış kampanya başvurusu içindir.");

           
            decimal? rawRate = await _db.Campaigns
                .AsNoTracking()
                .Where(c => c.Id == app.CampaignId.Value)
                .Select(c => (decimal?)c.FaizOrani)  
                .FirstOrDefaultAsync();

            if (rawRate is null)
                throw new InvalidOperationException("Kampanya bulunamadı veya faiz alanı NULL.");

            decimal faizYuzde = NormalizeToPercent(rawRate.Value);

            if (faizYuzde <= 0m)
                throw new InvalidOperationException("Geçersiz kampanya faiz oranı.");

         
            var request = new CreditRequestDto
            {
                KrediTutari = app.CreditAmount,
                Vade = app.CreditTerm,
                FaizOrani = faizYuzde
            };

            var response = _creditService.Hesapla(request);

           
            var html = _pdfService.GenerateInstallmentHtml(app, response.Installments);
            var pdfBytes = _pdfService.GeneratePdf(html);

            await _emailService.SendEmailWithAttachmentAsync(
                app.Email,
                "Kredi Başvurunuz Alındı",
                $"Merhaba {app.FullName},\n\nBaşvurunuz alınmıştır. Kampanya faiz oranınız: {faizYuzde:0.00}%. " +
                "Ödeme planınız ekte yer almaktadır.",
                pdfBytes,
                "Kredi_Odeme_Plani.pdf"
            );
        }

        private static decimal NormalizeToPercent(decimal v)
        {
            // 0.0189 gibi "oran" gelirse 1.89'a çevir; 1.89 gibi "yüzde" ise aynen bırak.
            return v <= 0.1m ? v * 100m : v;
        }

        public async Task SendStatusUpdateAsync(CreditApplication app)
        {
            var statusText = app.Status switch
            {
                CreditApplicationStatus.Approved => "Onaylandı",
                CreditApplicationStatus.Rejected => "Reddedildi",
                CreditApplicationStatus.Waiting => "Beklemede",
                _ => "Bilinmiyor"
            };

            await _emailService.SendEmailAsync(
                app.Email,
                "Kredi Başvuru Durumu",
                $"Sayın {app.FullName},\n\nBaşvurunuzun durumu güncellenmiştir: {statusText}"
            );
        }


        public async Task SendCustomerApplicationReceivedAsync(CustomerApplication app)
        {
            await _emailService.SendEmailAsync(
                app.Email,
                "Müşteri Başvurunuz Alındı",
                $"Merhaba {app.Name} {app.SurName},\n\n{app.BankName} bankasına müşteri olma başvurunuz başarıyla alınmıştır."
            );
        }

        public async Task SendCustomerStatusUpdateAsync(CustomerApplication app)
        {
            var statusText = app.Status switch
            {
                CustomerApplicationStatus.Approved => "Onaylandı",
                CustomerApplicationStatus.Rejected => "Reddedildi",
                CustomerApplicationStatus.Pending => "Beklemede",
                _ => "Bilinmiyor"
            };

            await _emailService.SendEmailAsync(
                app.Email,
                $"Müşteri Başvuru Durumu - {app.BankName}",
                $"Sayın {app.Name} {app.SurName},\n\nBaşvurunuzun durumu güncellenmiştir: {statusText}"
            );
        }
    }
}
