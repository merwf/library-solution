using Library.Business;
using Library.Business.Concrete;
using Library.Business.Interfaces;
using Library.Core.DTOs;
using System;

namespace LibraryApplication
{
    public class Program
    {
        static void Main(string[] args)
        {

            if (args == null || args.Length != 3)
            {
                PrintUsage();
                return;
            }
            String fee = "";
            try
            {
                // 1. Provider örneðini oluþturuyoruz
                ICountrySettingProvider settingProvider = new CountrySettingProvider();

                // 2. PenaltyFeeCalculator'a provider enjekte ediliyor (DIP)
                IPenaltyFeeCalculator calculator = new PenaltyFeeCalculator(settingProvider);

                // 3. Hesaplama yapýlýyor
                PenaltyResultDto result = calculator.Calculate(args[0], args[1], args[2]);

                // FormattedResult metnini alýyoruz (Örn: "5.25 TRY" ya da "Error: ...")
                fee = result.FormattedResult;
            }
            catch (Exception e)
            {
                PrintErrorMessage(e);
            }
            PrintResultMessage(fee);

        }

        private static void PrintUsage()
        {
            Console.WriteLine("Please provide these parameters (without brackets) : <CountryCode> <DateStart> <DateEnd>");
            Console.WriteLine(@"Example: LibraryApplication.exe tr-TR 23.11.2009 30.11.2009");
            PrintAnyKeyMessage();
            Console.ReadKey();
        }

        private static void PrintAnyKeyMessage()
        {
            Console.WriteLine("Press any key to continue");
        }

        private static void PrintResultMessage(string fee)
        {
            Console.WriteLine("Penalty Fee is {0}", fee);
            PrintAnyKeyMessage();
            Console.ReadKey();
        }

        private static void PrintErrorMessage(Exception e)
        {
            Console.WriteLine("Exception : " + e.Message);
            Console.WriteLine("Stacktrace : ");
            Console.WriteLine(e.StackTrace);
            PrintAnyKeyMessage();
            Console.ReadKey();
        }
    }
}