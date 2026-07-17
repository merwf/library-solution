using System;
using Library.Business;

namespace LibrarySolution.Tests
{
    public class PenaltyFeeCalculatorTests
    {
        private readonly PenaltyFeeCalculator _calculator;

        public PenaltyFeeCalculatorTests()
        {
            _calculator = new PenaltyFeeCalculator();
        }

        [Fact]
        public void Calculate_TR_ThresholdValue_ReturnsZeroFee()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "18.11.2009";
            string expected = "0.00 TRY";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Calculate_TR_OneDayPenalty_ReturnsCorrectFee()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "19.11.2009";
            string expected = "5,25"; // Kültürel uyuşmazlık riskine karşı sadece sayı değeri

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Contains(expected, result);
            Assert.Contains("TRY", result);
        }

        [Fact]
        public void Calculate_TR_WeekendTransition_CountsBusinessDaysCorrectly()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "23.11.2009";
            string expected = "15,75";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Contains(expected, result);
            Assert.Contains("TRY", result);
        }

        [Fact]
        public void Calculate_TR_WithHoliday_ExcludesHolidayCorrectly()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "30.11.2009";
            string expected = "26,25";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Contains(expected, result);
            Assert.Contains("TRY", result);
        }

        [Fact]
        public void Calculate_AE_DifferentWeekend_CalculatesCorrectly()
        {
            string countryCode = "ar-AE";
            string startDate = "16.11.2009";
            string endDate = "23.11.2009";
            string expected = "16.00 AED";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Calculate_InvalidCountryCode_ReturnsErrorMessage()
        {
            string countryCode = "xx-XX";
            string startDate = "16.11.2009";
            string endDate = "30.11.2009";
            string expectedErrorMessage = "Error: Country configuration not found.";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Equal(expectedErrorMessage, result);
        }

        [Fact]
        public void Calculate_StartDateLaterThanEndDate_ReturnsErrorMessage()
        {
            string countryCode = "tr-TR";
            string startDate = "20.11.2009";
            string endDate = "16.11.2009";
            string expectedErrorMessage = "Error: Start date cannot be later than end date.";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Equal(expectedErrorMessage, result);
        }

        [Fact]
        public void Calculate_SameDay_ReturnsZeroFee()
        {
            string countryCode = "tr-TR";
            string startDate = "16.11.2009";
            string endDate = "16.11.2009";
            string expected = "0.00 TRY";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Calculate_OnlyWeekend_ReturnsZeroFee()
        {
            string countryCode = "tr-TR";
            string startDate = "21.11.2009";
            string endDate = "22.11.2009";
            string expected = "0.00 TRY";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Calculate_AE_HolidayAndWeekendOverlap_CalculatesCorrectly()
        {
            // BAE için Cuma-Cumartesi hafta sonu ve 25-26-27 Kasım tatillerini içeren senaryo
            string countryCode = "ar-AE";
            string startDate = "16.11.2009";
            string endDate = "30.11.2009";
            string expected = "40.00 AED";

            string result = _calculator.Calculate(countryCode, startDate, endDate);

            Assert.Equal(expected, result);
        }
    }
}