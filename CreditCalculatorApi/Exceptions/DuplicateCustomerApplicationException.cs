namespace CreditCalculatorApi.Exceptions
{
    public class DuplicateCustomerApplicationException:BadRequestException
    {
        public DuplicateCustomerApplicationException(string identityNumber, string bankName)
            : base($"Bu kimlik numarası ({identityNumber}) ve banka ({bankName}) ile daha önce başvuru yapılmıştır.")
        {
        }
    }
}
