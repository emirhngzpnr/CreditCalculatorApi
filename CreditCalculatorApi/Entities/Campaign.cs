using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CreditCalculatorApi.Entities.Enums;

namespace CreditCalculatorApi.Entities
{
    public class Campaign
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }      
       
     
        public CreditType CreditType { get; set; }

       public int MinVade { get; set; } 
        public int MaxVade { get; set; } 
        public decimal MinKrediTutar { get; set; }
        public decimal MaxKrediTutar { get; set; } 
        public DateTime RecordTime { get; set; }  
        public DateTime BaslangicTarihi { get; set; } 
        public DateTime BitisTarihi { get; set; }
        public string Description { get; set; } = string.Empty; 
        public CampaignStatus CampaignStatus { get; set; } = CampaignStatus.Active; 
        public double FaizOrani { get; set; } // Faiz oranı


        public int BankId { get; set; }
        [ForeignKey(nameof(BankId))]
        public Bank? Bank { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}


  



  

  

  


