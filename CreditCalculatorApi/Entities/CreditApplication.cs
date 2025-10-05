using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CreditCalculatorApi.Entities.Enums;

namespace CreditCalculatorApi.Entities
{
    /// <summary>
    /// Kullanıcıdan kredi başvurusu esnasında istediğimiz bilgileri tutan sınıf.
    /// </summary>
    public class CreditApplication
    {
        public int Id { get; set; }
        
        public string FullName { get; set; }// Ad Soyad
        
        public string Email { get; set; }              // E-mail
        public string PhoneNumber { get; set; }        // Telefon numarası


        public string BankName { get; set; }           // Banka ismi
       
      
          public CreditType CreditType { get; set; } // Kredi tipi (İhtiyaç, Konut, Araç vs.)
        
        public decimal CreditAmount { get; set; }      // Kredi tutarı
        
        public int CreditTerm { get; set; }            // Vade
      
       
        public decimal MonthlyIncome { get; set; }     // Kullanıcının aylık geliri
        
        public string? RiskStatus { get; set; }
        public CreditApplicationStatus Status { get; set; } = CreditApplicationStatus.Waiting;
        public DateTime RecordTime { get; set; } = DateTime.UtcNow;
        public int? CampaignId { get; set; }

        [ForeignKey(nameof(CampaignId))]
        public Campaign? Campaign { get; set; }
        public string? UserNumber { get; set; }=string.Empty;
       







    }
}
