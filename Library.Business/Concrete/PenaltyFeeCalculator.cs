using Library.Business.Interfaces;
using Library.Core.DTOs;
using LibraryConfigUtilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Library.Business.Concrete
{
    public class PenaltyFeeCalculator : IPenaltyFeeCalculator
    {
        private readonly ICountrySettingProvider _settingProvider;

        /// <summary>
        /// Dependency Injection (DIP) prensibine uygun kurucu metot.
        /// </summary>
        public PenaltyFeeCalculator(ICountrySettingProvider settingProvider)
        {
            _settingProvider = settingProvider ?? throw new ArgumentNullException(nameof(settingProvider));
        }

        public PenaltyResultDto Calculate(string countryCode, string startDateStr, string endDateStr)
        {
            var settings = _settingProvider.GetCountrySettings();

            // 1. Ülke konfigürasyonunu Culture özelliðine göre buluyoruz
            var countrySetting = settings?.FirstOrDefault(c =>
                c.Culture.Equals(countryCode, StringComparison.OrdinalIgnoreCase));

            // Hata durumunda:
            if (countrySetting == null)
            {
                return new PenaltyResultDto { IsError = true, ErrorMessage = "Country configuration not found." };
            }

            // Ülkeye ait kültür bilgisini oluþturuyoruz
            CultureInfo culture = new CultureInfo(countrySetting.Culture);

            // 2. Tarih formatý dönüþümleri ve geçerlilik kontrolleri
            if (!DateTime.TryParse(startDateStr, culture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParse(endDateStr, culture, DateTimeStyles.None, out DateTime endDate))
            {
                return new PenaltyResultDto { IsError = true, ErrorMessage = "Invalid date format for the selected country." };
            }

            if (startDate > endDate)
            {
                return new PenaltyResultDto { IsError = true, ErrorMessage = "Start date cannot be later than end date." };
            }

            // 3. Ýþ günü sayýsýnýn hesaplanmasý
            int businessDays = GetBusinessDays(startDate, endDate, countrySetting);

            // 4. Ceza hesaplama mantýðý
            int allowedDays = countrySetting.PenaltyAppliesAfter;

            if (businessDays <= allowedDays)
            {
                return new PenaltyResultDto
                {
                    Amount = 0,
                    Currency = countrySetting.Currency
                };
            }

            int penaltyDays = businessDays - allowedDays;

            // Kültüre göre günlük cezayý ondalýklý sayýya çeviriyoruz
            decimal dailyFee = Convert.ToDecimal(countrySetting.DailyPenaltyFee, culture);
            decimal totalPenalty = penaltyDays * dailyFee;

            return new PenaltyResultDto
            {
                Amount = totalPenalty,
                Currency = countrySetting.Currency
            };
        }

        public static int GetBusinessDays(DateTime start, DateTime end, Country country)
        {
            int businessDayCount = 0;

            var weekendDays = country.WeekendList.Select(w => Convert.ToInt32(w)).ToList();
            var holidayDates = country.HolidayList.Select(h => h.Date.Date).ToList();

            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                int dayOfWeekValue = (int)date.DayOfWeek;
                if (weekendDays.Contains(dayOfWeekValue))
                {
                    continue;
                }

                if (holidayDates.Contains(date.Date))
                {
                    continue;
                }

                businessDayCount++;
            }

            return businessDayCount;
        }
    }
}