using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace JackCompiler
{
    public class JackFileService
    {
        #region Fields

        private const string JackFileExtension = @".jack";

        #endregion

        #region Constructos

        public JackFileService(string filePath)
        {
            if (File.Exists(filePath) == false || IsJackFile(filePath) == false)
            {
                throw new FileNotFoundException();
            }

            this.ReadAllLines = File.ReadAllLines(filePath).Select(l => l.Trim()).ToArray();
            this.AllCodeLines = GetJackCodeLines(this.ReadAllLines).ToArray();
        }

        #endregion

        #region Properties

        public int CurrentCodeLineNumber { get; private set; }

        public IReadOnlyCollection<string> ReadAllLines { get; }

        public IReadOnlyCollection<string> AllCodeLines { get; }

        #endregion

        #region Public Methods

        public string ReadNextCodeLine()
        {
            ++this.CurrentCodeLineNumber;
            return (this.CurrentCodeLineNumber >= this.AllCodeLines.Count) ? 
                       null : this.AllCodeLines.Skip(this.CurrentCodeLineNumber).First();
        }

        public static bool IsJackFile(string filePath)
        {
            return Path.GetExtension(filePath) == JackFileExtension;
        }

        #endregion

        #region Private Methods

        private static IEnumerable<string> GetJackCodeLines(IReadOnlyCollection<string> allFileLines)
        {
            const string BlankLinePattern = "^$";

            // read except blank line
            var readLines = allFileLines.Where(l => Regex.IsMatch(l, BlankLinePattern) == false).ToArray();           
            return Comment.RemoveAllComments(readLines);
        }

        #endregion

        #region Nested objects

        private static class Comment
        {
            private const string CommentOneLine = "^//.*";
            private const string CommentBeginSymbol = @"/*";
            private const string CommentBeginSymbolForApi = @"/**";
            private const string CommentEndSymbol = @"*/";
            private const string CommentForEndOfLine = "//.*$|/\\*.*\\*/$|/\\*\\*.*\\*/$";

            #region Internal Methods

            internal static IEnumerable<string> RemoveAllComments(IReadOnlyCollection<string> targetLines)
            {
                var removedLines = new List<string>();

                int usedLineCount = 0;
                while (ExistsNextLine())
                {
                    var line = targetLines.Skip(usedLineCount).First();

                    if (IsOneLineComment(line))
                    {
                        ++usedLineCount;
                    }
                    else if (HasBegunMultiLineComments(line))
                    {
                        usedLineCount += CountUpMultiCommentLine(usedLineCount, targetLines);
                    }
                    else
                    {
                        removedLines.Add(RemoveCommentForEndOfLine(line));
                        ++usedLineCount;
                    }
                }

                return removedLines;

                bool ExistsNextLine()
                {
                    return usedLineCount < targetLines.Count();
                }
            }

            #endregion

            #region Private Methods

            private static bool IsOneLineComment(string line)
            {
                if (Regex.IsMatch(line, CommentOneLine))
                {
                    return true;
                }

                if (line.StartsWith(CommentBeginSymbol) || line.StartsWith(CommentBeginSymbolForApi))
                {
                    return line.EndsWith(CommentEndSymbol);
                }

                return false;
            }

            private static bool HasBegunMultiLineComments(string line)
            {
                if (IsOneLineComment(line))
                {
                    return false;
                }

                if (line.StartsWith(CommentBeginSymbol) || line.StartsWith(CommentBeginSymbolForApi))
                {
                    return !line.EndsWith(CommentEndSymbol);
                }

                return false;
            }

            private static string RemoveCommentForEndOfLine(string line)
            {
                return Regex.Replace(line, CommentForEndOfLine, "");
            }

            private static int CountUpMultiCommentLine(int startingCommentLineCount, IReadOnlyCollection<string> readLines)
            {
                var line = readLines.Skip(startingCommentLineCount).First();
                if (line.EndsWith(CommentEndSymbol))
                {
                    return 1;
                }

                int commentLineCount = 1;
                while (line.EndsWith(CommentEndSymbol) == false)
                {
                    line = readLines.Skip(startingCommentLineCount + commentLineCount).First();
                    ++commentLineCount;
                }

                return commentLineCount;
            }

            #endregion

        }

        #endregion
    }
}
