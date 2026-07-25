using Library.Core;

namespace Library.Business
{
    public interface IPenaltyFeeCalculator
    {
        PenaltyResultDto Calculate(string countryCode, string startDateStr, string endDateStr);
    }
}
