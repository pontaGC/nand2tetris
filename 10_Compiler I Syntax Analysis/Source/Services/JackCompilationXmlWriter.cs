using System.Collections.Generic;
using System.IO;

using JackCompiler.Interfaces;

namespace JackCompiler.Services
{
    internal class JackCompilationXmlWriter : ICompilationXmlWriter
    {
        #region Fields

        private readonly List<string> writeLines = new List<string>();

        #endregion

        #region Constructors

        internal JackCompilationXmlWriter()
        {
        }

        #endregion

        #region Properties

        public IReadOnlyCollection<string> WriteLines => this.writeLines.AsReadOnly();

        #endregion

        #region Public Methods

        public void WriteLineSyntaxTag(JackSyntaxType type, XmlTagTypes tagTypes)
        {
            this.writeLines.Add(this.GetSyntaxXmlTag(type, tagTypes));
        }

        public void WriteLineTokenTag(IJackToken token)
        {
            this.writeLines.Add(this.ConvertTokenToXmlTag(token));
        }

        public string ConvertTokenToXmlTag(IJackToken token)
        {
            var tagName = JackTokenConstants.TokenTypeXmlTagNames[token.Type];
            return $"{GetStartTag(tagName)} {GetXmlFormattedTokenValue(token)} {GetEndTag(tagName)}";
        }

        public string GetSyntaxXmlTag(JackSyntaxType type, XmlTagTypes tagTypes)
        {
            var tagName = JackCompileEngineConstants.SyntaxXmlTagNames[type];
            return tagTypes == XmlTagTypes.StartTag ? GetStartTag(tagName) : GetEndTag(tagName);
        }

        public void SaveOutputs(string outputFilePath)
        {
            if (File.Exists(outputFilePath))
            {
                File.WriteAllLines(outputFilePath, this.writeLines);
            }
        }

        #endregion

        #region Private Methods

        private static string GetStartTag(string tagName)
        {
            return $"<{tagName}>";
        }

        private static string GetEndTag(string tagName)
        {
            return $"</{tagName}>";
        }

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
