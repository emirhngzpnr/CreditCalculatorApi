namespace CreditCalculatorApi.Exceptions
{
    public class DuplicateEmailException:ApplicationException
    {
        public DuplicateEmailException()
           : base("Bu e-posta adresiyle daha önce başvuru yapılmıştır.")
        {
        }
    }
}
