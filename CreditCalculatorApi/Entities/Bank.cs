using System.ComponentModel.DataAnnotations;

namespace CreditCalculatorApi.Entities
{
    public class Bank
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }=string.Empty;
        public ICollection<Campaign> Campaigns { get; set; }=new List<Campaign>();
    




    }

}
