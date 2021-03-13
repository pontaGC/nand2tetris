using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using JackCompiler.Interfaces;
using JackCompiler.Services;

namespace JackCompiler
{
    internal class JackTokenizer : IJackTokenizer
    {
        #region Fields

        private string nextTokenWord = string.Empty;
        private Queue<string> currentLineWords; // split by whitespaces
        private readonly JackFileService jackFileService;

        private IJackToken currentToken = new JackToken(JackTokenType.Unknown, string.Empty);
        private IJackToken nextToken;

        #endregion

        #region Constructors

        internal JackTokenizer(string filePath)
        {
            this.jackFileService = new JackFileService(filePath);
            this.currentLineWords = SplitOneLineToWords(this.jackFileService.AllCodeLines.First());
            this.nextToken = this.GetNextToken();
        }

        #endregion

        #region Public Methods

        public bool HasMoreTokens()
        {
            return this.nextToken != null;
        }

        public void Advance()
        {
            this.currentToken = this.nextToken;
            this.nextToken = this.GetNextToken();
        }

        public JackTokenType TokenType()
        {
            return this.currentToken.Type;
        }

        public string KeyWord()
        {
            return this.currentToken.Value;
        }

        public string Symbol()
        {
            return this.currentToken.Value;
        }

        public string Identifier()
        {
            return this.currentToken.Value;
        }

        public int IntVal()
        {
            var value = int.Parse(this.currentToken.Value);
            if (IsIntegerConstantWithinRange(value))
            {
                return value;
            }

            return -int.MaxValue;
        }

        public string StringVal()
        {
            return this.currentToken.Value;
        }

        #endregion

        #region Private Methods

        private JackToken GetNextToken()
        {
            this.AdvanceNextTokenizingWord();

            if (this.nextTokenWord == string.Empty)
            {
                return null;
            }

            foreach (var tokenType in JackTokenConstants.TokenRegexTable.Keys)
            {
                if (TryFindFirstTerminal(this.nextTokenWord, tokenType, out var terminal) == false)
                {
                    continue;
                }

                if (tokenType == JackTokenType.StringConstant)
                {
                    this.RemoveFoundTerminalFromWord($"\"{terminal}\"");
                    return new JackToken(tokenType, terminal);
                }

                if (this.IsSameTokenConsecutive(tokenType, terminal) == false)
                {
                    this.RemoveFoundTerminalFromWord(terminal);
                    return new JackToken(tokenType, terminal);
                }
            }

            return new JackToken(JackTokenType.Unknown, string.Empty);
        }

        private void AdvanceNextTokenizingWord()
        {
            if (this.nextTokenWord != string.Empty)
            {
                return;
            }

            if (this.currentLineWords.Any())
            {
                this.nextTokenWord = this.currentLineWords.Dequeue();
                return;
            }

            var nextCode = this.jackFileService.ReadNextCodeLine();
            if (nextCode != null)
            {
                this.currentLineWords = SplitOneLineToWords(nextCode);
                this.nextTokenWord = this.currentLineWords.Dequeue();
            }
        }

        private void RemoveFoundTerminalFromWord(string terminal)
        {
            // replace first match pattern to string.Empty
            this.nextTokenWord = new Regex(Regex.Escape(terminal))
                                            .Replace(this.nextTokenWord, "", 1);
        }

        private bool IsSameTokenConsecutive(JackTokenType tokenType, string foundTokenValue)
        {
            if (tokenType != JackTokenType.Keyword)
            {
                return false;
            }

            return this.currentToken.Value == foundTokenValue;
        }

        private static bool TryFindFirstTerminal(string targetWord, JackTokenType tokenType, out string terminal)
        {
            if (tokenType == JackTokenType.StringConstant 
                && JackTokenConstants.StringConstantRegex.IsMatch(targetWord))
            {
                terminal = targetWord.Replace("\"", "");
                return true;
            }

            var regex = JackTokenConstants.TokenRegexTable[tokenType];
            terminal = regex.Match(targetWord).Value;

            return terminal != string.Empty;
        }

        private static bool IsIntegerConstantWithinRange(int value)
        {
            return (JackTokenConstants.IntegerConstantRange.Min <= value
                    && value <= JackTokenConstants.IntegerConstantRange.Max);
        }


        #region Split line to words

        private static Queue<string> SplitOneLineToWords(string line)  // line is trimmed and removed comment
        {
            if (ExistsStringConstantInLine(line, out var stringConstant))
            {
                return SplitOneLineContainsStringConstantToWords(line, stringConstant);
            }

            // ignore whitespaces
            var words = Regex.Matches(line, "\\S+").Cast<Match>().Select(m => m.Value);
            return new Queue<string>(words);
        }

        private static bool ExistsStringConstantInLine(string line, out string stringConstant)
        {
            stringConstant = Regex.Match(line, JackTokenConstants.StringConstantPattern).Value;
            return stringConstant != string.Empty;
        }

        private static Queue<string> SplitOneLineContainsStringConstantToWords(string line, string stringConstant)
        {
            var pattern = $"^(?<previous>.*){JackTokenConstants.StringConstantPattern}(?<following>.*)$";
            var match = Regex.Match(line, pattern);

            var previousWords = Regex.Matches(match.Groups["previous"].Value, "\\S+")
                                                .Cast<Match>().Select(m => m.Value).ToList();

            var followingWords = Regex.Matches(match.Groups["following"].Value, "\\S+")
                                              .Cast<Match>().Select(m => m.Value).ToArray();

            previousWords.Add(stringConstant);
            previousWords.AddRange(followingWords);

            return new Queue<string>(previousWords);
        }

        #endregion

        #endregion

    }
}
