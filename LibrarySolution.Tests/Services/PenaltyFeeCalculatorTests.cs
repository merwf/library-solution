using Library.Business.Concrete;
using Library.Core.DTOs;
using System;
using Xunit;

namespace LibrarySolution.Tests.Services
{
    public class PenaltyFeeCalculatorTests
    {
        private readonly PenaltyFeeCalculator _calculator;

        public PenaltyFeeCalculatorTests()
        {
            _calculator = new PenaltyFeeCalculator();
        }

        // Türkiye için muafiyet sınırı (örn: ilk 3 gün) içindeki teslimlerde ceza tutarının 0 TRY olduğunu doğrular.
        [Fact]
        public void Calculate_TR_ThresholdValue_ReturnsZeroFee()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "18.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.False(result.IsError);
            Assert.Equal(0m, result.Amount);
            Assert.Equal("TRY", result.Currency);
        }

        // Türkiye için 1 günlük gecikme cezasının doğru tutarda (5.25 TRY) hesaplandığını doğrular.
        [Fact]
        public void Calculate_TR_OneDayPenalty_ReturnsCorrectFee()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "19.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.False(result.IsError);
            Assert.Equal(5.25m, result.Amount);
            Assert.Equal("TRY", result.Currency);
        }

        // Hafta sonuna (Cumartesi-Pazar) denk gelen günlerin düşülerek sadece iş günleri üzerinden ceza hesaplandığını doğrular.
        [Fact]
        public void Calculate_TR_WeekendTransition_CountsBusinessDaysCorrectly()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "23.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.False(result.IsError);
            Assert.Equal(15.75m, result.Amount);
            Assert.Equal("TRY", result.Currency);
        }

        // Resmi tatil günlerinin ceza hesabından doğru şekilde muaf tutulduğunu (düşüldüğünü) doğrular.
        [Fact]
        public void Calculate_TR_WithHoliday_ExcludesHolidayCorrectly()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "30.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.False(result.IsError);
            Assert.Equal(26.25m, result.Amount);
            Assert.Equal("TRY", result.Currency);
        }

        // Birleşik Arap Emirlikleri (BAE) gibi hafta sonu günleri farklı olan (Cuma-Cumartesi) ülkeler için hesaplamanın doğru yapıldığını doğrular.
        [Fact]
        public void Calculate_AE_DifferentWeekend_CalculatesCorrectly()
        {
            string countryCode = "ar-AE";
            string startDate = "16.11.2009";
            string endDate = "23.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.False(result.IsError);
            Assert.Equal(16.00m, result.Amount);
            Assert.Equal("AED", result.Currency);
        }

        // Sistemde tanımlı olmayan geçersiz bir ülke kodu gönderildiğinde uygun hata mesajının döndüğünü doğrular.
        [Fact]
        public void Calculate_InvalidCountryCode_ReturnsErrorMessage()
        {
            string countryCode = "xx-XX";
            string startDate = "16.11.2009";
            string endDate = "30.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.True(result.IsError);
            Assert.Equal("Country configuration not found.", result.ErrorMessage);
        }

        // Başlangıç tarihinin bitiş tarihinden sonra olduğu mantıksız tarih senaryolarında hata döndüğünü doğrular.
        [Fact]
        public void Calculate_StartDateLaterThanEndDate_ReturnsErrorMessage()
        {
            string countryCode = "tr-TR";
            string startDate = "20.11.2009";
            string endDate = "16.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.True(result.IsError);
            Assert.Equal("Start date cannot be later than end date.", result.ErrorMessage);
        }

        // Kitabın aynı gün içinde alınıp iade edilmesi durumunda ceza tutarının 0 TRY çıktığını doğrular.
        [Fact]
        public void Calculate_SameDay_ReturnsZeroFee()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "16.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.False(result.IsError);
            Assert.Equal(0m, result.Amount);
            Assert.Equal("TRY", result.Currency);
        }

        // Sadece hafta sonu tarihlerini kapsayan bir aralıkta ceza tutarının 0 TRY çıktığını doğrular.
        [Fact]
        public void Calculate_OnlyWeekend_ReturnsZeroFee()
        {
            string countryCode = "tr-TR";
            string startDate = "21.11.2009";
            string endDate = "22.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.False(result.IsError);
            Assert.Equal(0m, result.Amount);
            Assert.Equal("TRY", result.Currency);
        }

        // BAE için hem resmi tatilin hem de hafta sonunun üst üste geldiği karmaşık senaryolarda çifte düşme yapılmadan doğru hesaplandığını doğrular.
        [Fact]
        public void Calculate_AE_HolidayAndWeekendOverlap_CalculatesCorrectly()
        {
            string countryCode = "ar-AE";
            string startDate = "16.11.2009";
            string endDate = "30.11.2009";

            PenaltyResultDto result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.False(result.IsError);
            Assert.Equal(40.00m, result.Amount);
            Assert.Equal("AED", result.Currency);
        }
    }
}