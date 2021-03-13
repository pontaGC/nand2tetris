using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JackCompiler.Interfaces;

namespace JackCompiler
{
    internal class VmWriter : IVmWriter
    {
        #region Fields

        private readonly List<string> vmCodeWriteLines = new List<string>();

        #endregion

        #region Constructors

        internal VmWriter()
        {
            
        }

        #endregion

        #region Properties

        public IReadOnlyCollection<string> VmCodeWriteLines => this.vmCodeWriteLines.AsReadOnly();

        #endregion

        #region Public methods

        public void WritePush(string vmSegment, int index)
        {
            if (VmConstants.AllSegments.Contains(vmSegment) == false)
            {
                Debug.WriteLine($"VmWriter.WritePush: {vmSegment}");
                return;
            }

            this.WriteVmCommand(VmConstants.VmCommands.Push, vmSegment, index);
        }

        public void WritePop(string vmSegment, int index)
        {
            if (VmConstants.AllSegments.Contains(vmSegment) == false)
            {
                Debug.WriteLine($"VmWriter.WritePop: {vmSegment}");
                return;
            }

            this.WriteVmCommand(VmConstants.VmCommands.Pop, vmSegment, index);
        }

        public void WriteArithmetic(string arithmeticCommand)
        {
            if (VmConstants.AllArithmetics.Contains(arithmeticCommand) == false)
            {
                Debug.WriteLine($"VmWriter.WriteArithmetic: {arithmeticCommand}");
                return;
            }

            this.WriteVmCommand(arithmeticCommand);
        }

        public void WriteLabel(string label)
        {
            this.WriteVmCommand(VmConstants.VmCommands.Label, label);
        }

        public void WriteGoto(string label)
        {
            this.WriteVmCommand(VmConstants.VmCommands.Goto, label);
        }

        public void WriteIf(string label)
        {
            this.WriteVmCommand(VmConstants.VmCommands.If, label);
        }

        public void WriteCall(string functionName, int numberOfArgs)
        {
            this.WriteVmCommand(VmConstants.VmCommands.Call, functionName, numberOfArgs);
        }

        public void WriteFunction(string functionName, int numberOfLocals)
        {
            this.WriteVmCommand(VmConstants.VmCommands.Function, functionName, numberOfLocals);
        }

        public void WriteReturn()
        {
            this.WriteVmCommand(VmConstants.VmCommands.Return);
        }

        public void Close()
        {
            this.vmCodeWriteLines.Clear();
        }

        // this method is not defined by the book.
        public void WriteLineComment(string comment)
        {
            // this.vmCodeWriteLines.Add($"// {comment}");
        }

        #endregion

        #region Private methods

        private void WriteVmCommand(string command)
        {
            this.vmCodeWriteLines.Add(command);
        }

        private void WriteVmCommand(string command, string label)
        {
            this.vmCodeWriteLines.Add($"{command} {label}");
        }

        private void WriteVmCommand(string command, string segment, int index)
        {
            this.vmCodeWriteLines.Add($"{command} {segment} {index}");
        }

        #endregion

    }

}
