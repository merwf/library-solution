using System;
using Library.Business;

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
                // Calculate metodunu çaðýrýyoruz
                fee = new PenaltyFeeCalculator().Calculate(args[0], args[1], args[2]);
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