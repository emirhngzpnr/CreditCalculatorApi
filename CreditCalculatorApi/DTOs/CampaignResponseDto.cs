namespace CreditCalculatorApi.DTOs
{
    public class CampaignResponseDto
    {
       public int Id { get; set; } // Unique identifier for the campaign
        public string CreditType { get; set; } = string.Empty; // Type of credit, e.g., "İhtiyaç", "Taşıt", "Konut"
        public int MinVade { get; set; } // Minimum term in months
        public int MaxVade { get; set; } // Maximum term in months
        public decimal MinKrediTutar { get; set; } // Minimum credit amount in Turkish Lira
        public decimal MaxKrediTutar { get; set; } // Maximum credit amount in Turkish Lira
        public DateTime RecordTime { get; set; } // Record creation time
        public DateTime BaslangicTarihi { get; set; } // Campaign start date
        public DateTime BitisTarihi { get; set; } // Campaign end date
        public string Description { get; set; } = string.Empty; // Description of the campaign
        public double FaizOrani { get; set; } // Interest rate for the campaign
        public int BankId { get; set; } // ID of the bank offering the campaign
        public string BankName { get; set; } = string.Empty; // Name of the bank offering the campaign

    }
}
