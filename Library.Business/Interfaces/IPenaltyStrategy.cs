using LibraryConfigUtilities;

namespace Library.Business.Interfaces
{
    public interface IPenaltyStrategy
    {
        bool AppliesTo(string countryCode);
        decimal CalculatePenalty(int businessDays, Country countrySetting);
    }
}