using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VMTranslator
{
    public class VMTranslator
    {
        #region Fields

        private bool isDirectorySource;        
        private CodeWriter codeWriter;
        private IReadOnlyCollection<string> vmAllVMFiles;

        #endregion

        #region Constructors

        public VMTranslator(string sourcePath)
        {
            isDirectorySource = IsDirectoryPath(sourcePath);

            this.codeWriter = new CodeWriter(this.SetAssemblerFilePath(sourcePath));
            this.vmAllVMFiles = this.GetAllVMFiles(sourcePath);
        }

        #endregion

        #region Public Methods

        public void Translate()
        {
            foreach (var vmFile in this.vmAllVMFiles)
            {
                this.codeWriter.SetFileName(Path.GetFileNameWithoutExtension(vmFile));
                CompileVMFiles(vmFile);
            }

            this.codeWriter.Close();
        }

        #endregion

        #region Private Methods

        private void CompileVMFiles(string vmFile)
        {
            var parser = new Parser(vmFile);

            while (parser.HasMoreCommands())
            {
                parser.Advance();

                var commandType = parser.CommandType();
                switch (commandType)
                {
                    case VMCommandType.C_ARITHMETIC:
                        this.codeWriter.WriteArithmetic(parser.Arg1());
                        break;
                    case VMCommandType.C_POP:
                    case VMCommandType.C_PUSH:
                        this.codeWriter.WritePushPop(parser.CurrentCommand, parser.Arg1(), parser.Arg2());
                        break;
                    // The following commands are implemented in Chapter 8
                    //case VMCommandType.C_FUNCTION:
                    //case VMCommandType.C_CALL:
                    //case VMCommandType.C_GOTO:
                    //case VMCommandType.C_IF:
                    //case VMCommandType.C_LABEL:
                    //case VMCommandType.C_RETURN:
                    //    break;
                    default:
                        break;
                }
            }
        }

        private IReadOnlyCollection<string> GetAllVMFiles(string sourcePath)
        {
            var allVMFiles = new List<string>();

            if(this.isDirectorySource == false)
            {
                allVMFiles.Add(sourcePath);
                return allVMFiles;
            }

            foreach (var fileName in Directory.GetFiles(sourcePath, @"*.vm"))
            {
                allVMFiles.Add(fileName);
            }

            return allVMFiles;
        }

        private string SetAssemblerFilePath(string sourcePath)
        {
            if (this.isDirectorySource)
            {
                var directoryPath = Path.GetDirectoryName(sourcePath);
                return $"{directoryPath}\\{Path.GetFileName(directoryPath)}.asm";
            }
            else
            {
                return Regex.Replace(sourcePath, @".vm", @".asm");
            }
        }

        private static bool IsDirectoryPath(string sourcePath)
        {
            return Path.GetExtension(sourcePath) == "";
        }

        #endregion
    }
}
