namespace JackCompiler
{
    internal class Program
    {
        private static void Main()
        {
            const string SourceDirectoryPath = @"./10/Square/";

            RunCompile(SourceDirectoryPath);
        }

        private static void RunCompile(string sourcePath)
        {
            var unused = new JackAnalyzer(sourcePath);
        }
    }
}
