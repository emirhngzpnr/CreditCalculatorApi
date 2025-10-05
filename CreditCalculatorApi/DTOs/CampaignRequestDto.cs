namespace CreditCalculatorApi.DTOs
{
    public class CampaignRequestDto
    {
        public int CreditType { get; set; } // Kredi tipi, örneğin 1: İhtiyaç Kredisi, 2: Taşıt Kredisi, 3: Konut Kredisi
        public int MinVade { get; set; } // Minimum vade süresi (ay olarak)
        public int MaxVade { get; set; } // Maksimum vade süresi (ay olarak)
        public decimal MinKrediTutar { get; set; } // Minimum kredi tutarı (TL cinsinden)
        public decimal MaxKrediTutar { get; set; } // Maksimum kredi tutarı (TL cinsinden)
        public DateTime BaslangicTarihi { get; set; } // Kampanya başlangıç tarihi
        public DateTime BitisTarihi { get; set; } // Kampanya bitiş tarihi
        public string Description { get; set; } = string.Empty; // Kampanya açıklaması
        public double FaizOrani { get; set; } // Faiz oranı
        public int BankId { get; set; } // Banka ID'si, kampanyanın hangi bankaya ait olduğunu belirtir
    }
}
