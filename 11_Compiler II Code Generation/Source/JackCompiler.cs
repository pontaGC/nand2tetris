using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JackCompiler
{
    internal class JackCompiler
    {
        #region Fields

        private const string SourceFileExtension = @"jack";

        private readonly bool isFilePath; // source path is file path

        #endregion

        #region Constructors

        internal JackCompiler(string sourcePath)
        {
            this.isFilePath = Path.GetExtension(sourcePath) != ""; 

            var sourceFilePaths = this.GetAllSourceFiles(sourcePath).ToArray();

            this.CompileAllJackFiles(sourceFilePaths);
        }

        #endregion

        #region Private methods

        private void CompileAllJackFiles(IReadOnlyCollection<string> sourceFilePaths)
        {
            var resultFilePaths = this.GetCompilingResultFilePaths(sourceFilePaths).ToArray();

            int compiledFileCount = 0;
            while (compiledFileCount < sourceFilePaths.Count)
            {
                var sourceFilePath = sourceFilePaths.Skip(compiledFileCount).First();
                var resultFilePath = resultFilePaths.Skip(compiledFileCount).First();

                var unused = new JackCompilationEngine(sourceFilePath, resultFilePath);

                ++compiledFileCount;
            }
        }

        #region Get file paths

        private IEnumerable<string> GetAllSourceFiles(string sourcePath)
        {
            return this.isFilePath ? new[] { sourcePath } 
                                : Directory.GetFiles(sourcePath, $"*.{SourceFileExtension}");
        }

        private IEnumerable<string> GetCompilingResultFilePaths(IReadOnlyCollection<string> sourceFilePaths)
        {
            var directoryName = Path.GetDirectoryName(sourceFilePaths.First());

            if (this.isFilePath)
            {
                return new[]
                           {
                               $"{directoryName}\\{Path.GetFileNameWithoutExtension(sourceFilePaths.First())}.vm"
                           };
            }

            return sourceFilePaths.Select(p =>
                                           $"{directoryName}\\{Path.GetFileNameWithoutExtension(p)}.vm");
        }

        #endregion

        #endregion
    }

}
