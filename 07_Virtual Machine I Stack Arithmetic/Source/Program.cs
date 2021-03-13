using System;

namespace VMTranslator
{
    public class Program
    {
        private static void Main()
        {
            string targetSouce = @"./07/MemoryAccess/StaticTest/";

            var vmTranslator = new VMTranslator(targetSouce);

            vmTranslator.Translate();

            Console.WriteLine(@"Enter any key...");
            Console.ReadKey();
        }

    }
}
