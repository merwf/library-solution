using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibraryConfigUtilities;

namespace LibraryBusiness
{
    public class PenaltyFeeCalculator
    {
        // App.config dosyasýndan gelen konfigürasyon listesi
        private List<Country> settingList = new LibrarySetting().LibrarySettingList;

        public PenaltyFeeCalculator() { }

        /// <summary>
        /// Ülke kodu ve tarih aralýðýna göre toplam cezayý hesaplar.
        /// </summary>
        public string Calculate(string countryCode, string startDateStr, string endDateStr)
        {
            // 1. Ülke konfigürasyonunu Culture özelliðine göre buluyoruz
            var countrySetting = settingList.FirstOrDefault(c =>
                c.Culture.Equals(countryCode, StringComparison.OrdinalIgnoreCase));

            if (countrySetting == null)
            {
                return "Error: Country configuration not found.";
            }

            // Ülkeye ait kültür bilgisini oluþturuyoruz
            CultureInfo culture = new CultureInfo(countrySetting.Culture);

            // 2. Tarih formatý dönüþümleri ve geçerlilik kontrolleri
            if (!DateTime.TryParse(startDateStr, culture, DateTimeStyles.None, out DateTime startDate) ||
                !DateTime.TryParse(endDateStr, culture, DateTimeStyles.None, out DateTime endDate))
            {
                return "Error: Invalid date format for the selected country.";
            }

            if (startDate > endDate)
            {
                return "Error: Start date cannot be later than end date.";
            }

            // 3. Ýþ günü sayýsýnýn hesaplanmasý
            int businessDays = GetBusinessDays(startDate, endDate, countrySetting);

            // 4. Ceza hesaplama mantýðý
            int allowedDays = countrySetting.PenaltyAppliesAfter;

            if (businessDays <= allowedDays)
            {
                return $"0.00 {countrySetting.Currency}";
            }

            int penaltyDays = businessDays - allowedDays;

            // Kültüre göre günlük cezayý ondalýklý sayýya çeviriyoruz
            decimal dailyFee = Convert.ToDecimal(countrySetting.DailyPenaltyFee, culture);
            decimal totalPenalty = penaltyDays * dailyFee;

            return $"{totalPenalty.ToString("F2", culture)} {countrySetting.Currency}";
        }

        /// <summary>
        /// DLL içindeki gerçek WeekendList ve HolidayList özelliklerine göre iþ günlerini sayar.
        /// </summary>
        public static int GetBusinessDays(DateTime start, DateTime end, Country country)
        {
            int businessDayCount = 0;

            // .Day alanýna gerek yok, doðrudan gelen deðeri int'e cast ediyoruz.
            var weekendDays = country.WeekendList.Select(w => Convert.ToInt32(w)).ToList();

            // String olmadýðý için .Trim() fonksiyonunu kaldýrýp doðrudan Date.Date ile gün bilgisini alýyoruz.
            var holidayDates = country.HolidayList.Select(h => h.Date.Date).ToList();

            // Baþlangýç gününden bitiþ gününe kadar döngü
            for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                // Hafta sonu kontrolü
                int dayOfWeekValue = (int)date.DayOfWeek;
                if (weekendDays.Contains(dayOfWeekValue))
                {
                    continue;
                }

                // Resmi tatil kontrolü (iki DateTime nesnesini doðrudan kýyaslama)
                if (holidayDates.Contains(date.Date))
                {
                    continue;
                }

                // Eðer engellere takýlmadýysa iþ günüdür
                businessDayCount++;
            }

            return businessDayCount;
        }
    }
}