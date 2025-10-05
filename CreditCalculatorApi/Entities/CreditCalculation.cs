namespace CreditCalculatorApi.Entities
{
    public class CreditCalculation
    {
        public int Id { get; set; }
        public decimal KrediTutari { get; set; }
        public int Vade { get; set; }
        public decimal FaizOrani { get; set; }
        public decimal AylikTaksit { get; set; }
        public decimal ToplamOdeme { get; set; }
        public DateTime HesaplamaTarihi { get; set; } = DateTime.UtcNow;
    }
}
