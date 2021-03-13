using System.Collections.Generic;

namespace JackCompiler.Interfaces
{
    public interface IVmWriter
    {
        #region Properties

        IReadOnlyCollection<string> VmCodeWriteLines { get; }

        #endregion

        #region Methods

        void WritePush(string vmSegment, int index);

        void WritePop(string vmSegment, int index);

        void WriteArithmetic(string arithmeticCommand);

        void WriteLabel(string label);

        void WriteGoto(string label);

        void WriteIf(string label);

        void WriteCall(string functionName, int numberOfArgs);

        void WriteFunction(string functionName, int numberOfLocals);

        void WriteReturn();

        void Close();

        // this method is not defined by the book.
        void WriteLineComment(string comment);

        #endregion
    }
}
