namespace CreditCalculatorApi.Helpers
{
    public class RiskCalculator
    {
        public static string Calculate(decimal creditAmount, int creditTerm, decimal monthlyIncome)
        {
            if (creditAmount <= 0 || creditTerm <= 0 || monthlyIncome <= 0)
                return "Unknown";

            decimal monthlyInstallment = creditAmount / creditTerm;
            decimal dti = monthlyInstallment / monthlyIncome;

            if (dti <= 0.4m)
                return "Safe";
            else if (dti <= 0.6m)
                return "Medium";
            else
                return "Risky";
        }
    }
}
