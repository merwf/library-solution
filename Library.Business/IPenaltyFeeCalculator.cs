namespace Library.Business
{
    public interface IPenaltyFeeCalculator
    {
        string Calculate(string countryCode, string startDateStr, string endDateStr);
    }
}
