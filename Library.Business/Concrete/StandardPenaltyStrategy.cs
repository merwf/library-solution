using Library.Business.Interfaces;
using LibraryConfigUtilities;
using System;
using System.Globalization;

namespace Library.Business.Concrete.Strategies
{
    public class StandardPenaltyStrategy : IPenaltyStrategy
    {
        public bool AppliesTo(string countryCode)
        {
            // Tüm ülkeler için varsayılan fallback stratejisi
            return true;
        }

        public decimal CalculatePenalty(int businessDays, Country countrySetting)
        {
            int allowedDays = countrySetting.PenaltyAppliesAfter;
            if (businessDays <= allowedDays)
                return 0m;

            int penaltyDays = businessDays - allowedDays;
            CultureInfo culture = new CultureInfo(countrySetting.Culture);
            decimal dailyFee = Convert.ToDecimal(countrySetting.DailyPenaltyFee, culture);

            return penaltyDays * dailyFee;
        }
    }
}