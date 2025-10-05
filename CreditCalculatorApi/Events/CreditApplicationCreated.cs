using CreditCalculatorApi.Entities.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace CreditCalculatorApi.Events
{
    [BsonIgnoreExtraElements] // fazladan alan gelirse yoksay
    public class CreditApplicationCreated
    {
        public int ApplicationId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string BankName { get; set; } = "";
        public string UserNumber { get; set; } = "";

        public CreditType CreditType { get; set; }
        public decimal CreditAmount { get; set; }
        public int CreditTerm { get; set; }
        public decimal MonthlyIncome { get; set; }
        public string? RiskStatus { get; set; }
        public string Status { get; set; } = "";
        public DateTime RecordTime { get; set; }
        public int? CampaignId { get; set; }
     
    }
}
