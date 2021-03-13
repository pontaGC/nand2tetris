namespace JackCompiler
{
    internal class Program
    {
        private static void Main()
        {
            const string Seven = @"./11/Seven/";
            const string ConvertToBin = @"./11/ConvertToBin/";
            const string Square = @"./11/Square/";
            const string Average = @"./11/Average/";
            const string Pong = @"./11/Pong/";
            const string ComplexArrays = @"./11/ComplexArrays/";

            RunCompile(ComplexArrays);
        }

        private static void RunCompile(string sourcePath)
        {
            var unused = new JackCompiler(sourcePath);
        }
    }
}
