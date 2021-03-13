using System.Collections.Generic;

namespace JackCompiler.Interfaces
{
    public enum XmlTagTypes
    {
        StartTag,
        EndTag
    }


    public interface ICompilationXmlWriter
    {
        #region Properties

        IReadOnlyCollection<string> WriteLines { get; }

        #endregion

        #region Methods

        void WriteLineSyntaxTag(JackSyntaxType type, XmlTagTypes tagTypes);

        void WriteLineTokenTag(IJackToken token);

        void SaveOutputs(string outputFilePath);

        string ConvertTokenToXmlTag(IJackToken token);

        string GetSyntaxXmlTag(JackSyntaxType type, XmlTagTypes tagTypes);

        #endregion

    }
}
