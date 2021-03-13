using System;

namespace VMTranslation
{
     public class Program
    {
        private static void Main()
        {
            string ts1 = @"./08/ProgramFlow/FibonacciSeries/";
            string ts2 = @"./08/FunctionCalls/SimpleFunction/";
            string ts3 = @"./08/FunctionCalls/FibonacciElement/";
            string ts4 = @"./08/FunctionCalls/StaticsTest/";
            string targetSouce = ts4;

            var vmTranslation = new VMTranslation(targetSouce);
            vmTranslation.Translate();

            Console.WriteLine(@"Enter any key...");
            Console.ReadKey();
        }
    }
}
