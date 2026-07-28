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
        // App.config dosyasýndan gelen konfigürasyon listesi
        private List<Country> _settingList;

        /// <summary>
        /// Varsayýlan kurucu metot. Konfigürasyon bilgilerini App.config / XML ayarlarýndan otomatik yükler.
        /// </summary>
        public PenaltyFeeCalculator() 
        {
            _settingList = new LibrarySetting().LibrarySettingList;
        }

        /// <summary>
        /// Unit Test veya baðýmsýz senaryolar için dýþarýdan özel ülke konfigürasyon listesi enjekte edilmesini saðlayan kurucu metot.
        /// </summary>
        /// <param name="settingList">Hesaplamalarda kullanýlacak özel ülke ve kural listesi.</param>
        public PenaltyFeeCalculator(List<Country> settingList)
        {
            _settingList = settingList ?? new LibrarySetting().LibrarySettingList;
        }

        /// <summary>
        /// Ülke kodu ve tarih aralýðýna göre toplam cezayý hesaplar.
        /// </summary>
        public PenaltyResultDto Calculate(string countryCode, string startDateStr, string endDateStr)
        {
            // 1. Ülke konfigürasyonunu Culture özelliðine göre buluyoruz
            var countrySetting = _settingList.FirstOrDefault(c =>
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