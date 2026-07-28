using Library.Core.DTOs;

namespace Library.Business.Interfaces
{
    public interface IPenaltyFeeCalculator
    {
        PenaltyResultDto Calculate(string countryCode, string startDateStr, string endDateStr);
    }
}
