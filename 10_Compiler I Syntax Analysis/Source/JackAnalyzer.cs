using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

using JackCompiler.Interfaces;
using JackCompiler.Services;

namespace JackCompiler
{
    internal class JackAnalyzer
    {
        #region Fields

        private const string SourceFileExtension = @"jack";

        private readonly bool isFilePath; // source path is file path

        #endregion

        #region Constructors

        internal JackAnalyzer(string sourcePath)
        {
            this.isFilePath = Path.GetExtension(sourcePath) != ""; 

            var sourceFilePaths = this.GetAllSourceFiles(sourcePath).ToArray();
            this.TokenizeAllJackFiles(sourceFilePaths);
            this.CompileAllJackFiles(sourceFilePaths);
            
        }

        #endregion

        #region Private Methods

        #region Tokenize

        private void TokenizeAllJackFiles(IReadOnlyCollection<string> sourceFilePaths)
        {
            var resultFilePaths = this.GetTokenizationResultFilePaths(sourceFilePaths).ToArray();
            
            int tokenizedFileCount = 0;
            while (tokenizedFileCount < sourceFilePaths.Count)
            {
                var sourceFilePath = sourceFilePaths.Skip(tokenizedFileCount).First();
                var resultFilePath = resultFilePaths.Skip(tokenizedFileCount).First();

                var jackTokens = TokenizeOneJackFile(sourceFilePath);

                SaveJackTokenizationResult(resultFilePath, jackTokens);

                ++tokenizedFileCount;
            }
        }

        private static IReadOnlyCollection<IJackToken> TokenizeOneJackFile(string jackFilePath)
        {
            var tokenCollector = new List<IJackToken>();

            var tokenizer = new JackTokenizer(jackFilePath);

            while (tokenizer.HasMoreTokens())
            {
                tokenizer.Advance();

                var tokenType = tokenizer.TokenType();

                IJackToken jackToken;
                switch (tokenType)
                {
                    case JackTokenType.Keyword:
                        jackToken = new JackToken(tokenType, tokenizer.KeyWord());
                        break;
                    case JackTokenType.Symbol:
                        jackToken = new JackToken(tokenType, tokenizer.Symbol());
                        break;
                    case JackTokenType.IntegerConstant:
                        jackToken = new JackToken(tokenType, tokenizer.IntVal().ToString());
                        break;
                    case JackTokenType.StringConstant:
                        jackToken = new JackToken(tokenType, tokenizer.StringVal());
                        break;
                    case JackTokenType.Identifier:
                        jackToken = new JackToken(tokenType, tokenizer.Identifier());
                        break;
                    default:
                        Debug.Assert(false, "Found an unknown token");
                        return null;
                }

                tokenCollector.Add(jackToken);
            }

            return tokenCollector;
        }

        #region Save tokenizing results

        private static void SaveJackTokenizationResult(string resultFilePath, IEnumerable<IJackToken> jackTokens)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));

            var rootElement = xmlDocument.CreateElement("tokens");
            xmlDocument.AppendChild(rootElement);

            foreach (var token in jackTokens)
            {
                var tokenValue = GetXmlFormattedTokenValue(token);

                var elementName = JackTokenConstants.TokenTypeXmlTagNames[token.Type];

                var xElement = xmlDocument.CreateElement(elementName);
                xElement.InnerText = tokenValue;

                rootElement.AppendChild(xElement);
            }

            xmlDocument.Save(resultFilePath);
        }

        #endregion

        #endregion

        #region CompileEngine

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




        #endregion

        #region Get file paths

        private IEnumerable<string> GetAllSourceFiles(string sourcePath)
        {
            return this.isFilePath ? new[] { sourcePath } 
                                : Directory.GetFiles(sourcePath, $"*.{SourceFileExtension}");
        }

        private IEnumerable<string> GetTokenizationResultFilePaths(IReadOnlyCollection<string> sourceFilePaths)
        {
            var directoryName = Path.GetDirectoryName(sourceFilePaths.First());

            if (this.isFilePath)
            {
                return new[] 
                {
                    $"{directoryName}\\MyParsingResults\\{Path.GetFileNameWithoutExtension(sourceFilePaths.First())}T.xml"
                };
            }

            return sourceFilePaths.Select(p =>
                $"{directoryName}\\MyParsingResults\\{Path.GetFileNameWithoutExtension(p)}T.xml");
        }

        private IEnumerable<string> GetCompilingResultFilePaths(IReadOnlyCollection<string> sourceFilePaths)
        {
            var directoryName = Path.GetDirectoryName(sourceFilePaths.First());

            if (this.isFilePath)
            {
                return new[]
                           {
                               $"{directoryName}\\MyParsingResults\\{Path.GetFileNameWithoutExtension(sourceFilePaths.First())}.xml"
                           };
            }

            return sourceFilePaths.Select(p =>
                $"{directoryName}\\MyParsingResults\\{Path.GetFileNameWithoutExtension(p)}.xml");
        }

        #endregion

        private static string GetXmlFormattedTokenValue(IJackToken jackToken)
        {
            if (jackToken.Type == JackTokenType.Symbol
                && JackTokenConstants.XMlEscapeFormatTable
                    .TryGetValue(jackToken.Value, out var formattedValue))
            {
                return formattedValue;
            }

            return jackToken.Value;
        }

        #endregion
    }

}
