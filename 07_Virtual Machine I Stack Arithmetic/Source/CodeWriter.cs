using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VMTranslator
{
    public class CodeWriter
    {
        #region Fields

        private static readonly Dictionary<string, (string Dest, string Comp, string Jump)>
            ArithmeticC_CommandTable = new Dictionary<string, (string Dest, string Comp, string Jump)>()
            {
                 { VMCommandConstants.Add,         ("D", "D+M", "") },
                 { VMCommandConstants.Subtruction, ("D", "M-D", "") },
                 { VMCommandConstants.Negation,    ("D", "-M",  "") },
                 { VMCommandConstants.And,         ("D", "D&M", "") },
                 { VMCommandConstants.Or,          ("D", "D|M", "") },
                 { VMCommandConstants.Not,         ("D", "!M",  "") },
            };

        private static readonly Dictionary<string, (string Dest, string Comp, string Jump)>
            RelationalC_CommandTable = new Dictionary<string, (string Dest, string Comp, string Jump)>()
            {
                 { VMCommandConstants.Equality,    ("",  "D",   "JEQ") },
                 { VMCommandConstants.Greater,     ("",  "D",   "JGT") },
                 { VMCommandConstants.Less,        ("",  "D",   "JLT") },
            };


        private static readonly Dictionary<string, string> VMSegmentVirtualRegisterSymbolTable = new Dictionary<string, string>()
        {
            { VMSegmentConstants.Local,    VirtualRegisterConstants.Local },
            { VMSegmentConstants.Argument, VirtualRegisterConstants.Argument },
            { VMSegmentConstants.This,     VirtualRegisterConstants.This },
            { VMSegmentConstants.That,     VirtualRegisterConstants.That },
        };

        private static readonly Regex labelPattern = new Regex("[a-zA-Z_\\.:][0-9a-zA-Z_\\.:]*");

        private StreamWriter assemblyFile;
        private string vmFileName = string.Empty;
        private int conditionalBranchCount;

        #endregion

        #region Constructors

        public CodeWriter(string assemblyFilePath)
        {
            this.assemblyFile = new StreamWriter(assemblyFilePath);
        }

        #endregion

        #region Public Methods

        public void SetFileName(string vmFileNameWithoutExtension)
        {
            this.vmFileName = vmFileNameWithoutExtension;
            this.assemblyFile.Flush(); // to prevent the buffer from filling up
        }

        public void Close()
        {
            this.WriteLine_Comment("Finish Hack program");
            this.WriteOutL_Command("END_PROGRAM");
            this.WriteOutA_Command("END_PROGRAM");
            this.WriteOutC_Command(comp: "0", jump: "JMP");

            this.assemblyFile.Dispose();
        }

        public void WriteArithmetic(string command)
        {
            switch (command)
            {
                case VMCommandConstants.Add:
                case VMCommandConstants.Subtruction:
                case VMCommandConstants.Equality:
                case VMCommandConstants.Greater:
                case VMCommandConstants.Less:
                case VMCommandConstants.And:
                case VMCommandConstants.Or:
                    this.WriteBinaryArithmetic(command);
                    break;
                case VMCommandConstants.Negation:
                case VMCommandConstants.Not:
                    this.WriteUnaryArithmetic(command);
                    break;
                default:
                    break;
            }
        }

        public void WritePushPop(string command, string segment, int index)
        {
            if (command == VMCommandConstants.Push)
            {
                if (segment == VMSegmentConstants.Constant)
                {
                    this.WritePushConstant(index);
                }
                else
                {
                    this.WritePushVirtualSegment(segment, index);
                }
            }
            else if (command == VMCommandConstants.Pop)
            {
                this.WritePop(segment, index);
            }
        }

        #endregion

        #region Private Methods

        #region Write arithmetic operations

        private void WriteUnaryArithmetic(string arithmeticCommand)
        {
            if (ArithmeticC_CommandTable.TryGetValue(arithmeticCommand, out var c_command) == false)
            {
                return;
            }

            this.WritePopToA_Register();
            this.WriteOutC_Command(dest: c_command.Dest, comp: c_command.Comp, jump: c_command.Jump);
            this.WritePushFromD_Register();
        }

        private void WriteBinaryArithmetic(string arithmeticCommand)
        {
            if (arithmeticCommand == VMCommandConstants.Equality
                || arithmeticCommand == VMCommandConstants.Greater
                || arithmeticCommand == VMCommandConstants.Less)
            {
                this.WriteRelationalOperation(arithmeticCommand);
            }
            else
            {
                this.WriteArithmeticOperation(arithmeticCommand);
            }
        }

        private void WriteArithmeticOperation(string arithmeticCommand)
        {
            if (ArithmeticC_CommandTable.TryGetValue(arithmeticCommand, out var c_command) == false)
            {
                return;
            }

            // arg1
            this.WritePopToA_Register();
            this.WriteOutC_Command(dest: "D", comp: "M");   

            // arg2
            this.WritePopToA_Register();

            this.WriteOutC_Command(dest: c_command.Dest, comp: c_command.Comp, jump: c_command.Jump);

            this.WritePushFromD_Register();
        }

        private void WriteRelationalOperation(string relationalCommand)
        {
            if (RelationalC_CommandTable.TryGetValue(relationalCommand, out var c_command) == false)
            {
                return;
            }

            this.WriteCondition(c_command.Dest, c_command.Comp, c_command.Jump);
        }

        #endregion

        #region Push operations

        private void WritePushConstant(int value)
        {
            this.WriteOutA_Command(value);
            this.WriteOutC_Command(dest: "D", comp: "A");
            this.WritePushFromD_Register();
        }

        private void WritePushVirtualSegment(string segment, int index)
        {
            this.WriteMemorySegmentMapping(segment, index);
            this.WriteOutC_Command(dest: "D", comp: "M");
            this.WritePushFromD_Register();
        }

        #endregion

        #region Pop operations

        private void WritePop(string segment, int index)
        {
            this.WritePopToA_Register();
            this.WriteOutC_Command(dest: "D", comp: "M");
            this.WriteMemorySegmentMapping(segment, index);
            this.WriteOutC_Command(dest: "M", comp: "D");
        }

        #endregion

        #region Conditional operations

        private void WriteCondition(string Dest, string Comp, string Jump)
        {
            this.WriteBinaryArithmetic(VMCommandConstants.Subtruction);
            this.WritePopToA_Register();
            this.WriteOutC_Command(dest: "D", comp: "M");

            var trueConditionLabel = $"Conditional{this.conditionalBranchCount}isTrue";
            var falseConditionLabel = $"Conditional{this.conditionalBranchCount}isFalse";

            this.WriteOutA_Command(trueConditionLabel);
            this.WriteOutC_Command(dest: Dest, comp: Comp, jump: Jump);

            this.WriteOutA_Command(falseConditionLabel);
            this.WriteOutC_Command(dest: "D", comp: "0");  // False is 0
            this.WriteOutC_Command(comp: "0", jump: "JMP");

            this.WriteOutL_Command(trueConditionLabel);
            this.WriteOutC_Command(dest: "D", comp: "-1"); // True is -1

            this.WriteOutL_Command(falseConditionLabel);

            this.WritePushFromD_Register();

            ++this.conditionalBranchCount;
        }

        #endregion

        #region Memory - Segment Mapping

        private void WriteMemorySegmentMapping(string segment, int index)
        {
            switch (segment)
            {
                case VMSegmentConstants.Argument:
                case VMSegmentConstants.Local:
                case VMSegmentConstants.This:
                case VMSegmentConstants.That:
                    this.WriteSegmentBaseAddressing(segment, index);                        // A= RAM[segment-baseaddress] + index
                    break;
                case VMSegmentConstants.Pointer:
                    this.WriteBaseAddressing(VirtualRegisterConstants.PointerBaseAddress, index); // A = pointer-baseaddress + index
                    break;
                case VMSegmentConstants.Temp:
                    this.WriteBaseAddressing(VirtualRegisterConstants.TempBaseAddress, index);    // A = temp-baseaddress + index
                    break;
                case VMSegmentConstants.Static:
                    this.WriteEntryStaticVariable(index);                                   // A = @XXX.index
                    break;
                default:
                    return;
            }
        }

        private void WriteSegmentBaseAddressing(string segment, int index)
        {
            if(VMSegmentVirtualRegisterSymbolTable.TryGetValue(segment, out var registerSymbol) == false)
            {
                return;
            }

            this.WriteOutA_Command(registerSymbol);                 
            this.WriteOutC_Command(dest: "A", comp: "M");

            foreach (var i in Enumerable.Range(0, index))
            {
                this.WriteOutC_Command(dest: "A", comp: "A+1");
            }
        }

        private void WriteBaseAddressing(int baseAddress, int index)
        {
            this.WriteOutA_Command(baseAddress + index); 
        }

        private void WriteEntryStaticVariable(int value)
        {
            this.WriteOutA_Command($"{this.vmFileName}.{value}");
        }

        #endregion
        
        #region Register Operations

        private void WritePushFromD_Register()
        {
            this.WriteOutA_Command(VirtualRegisterConstants.SP);
            this.WriteOutC_Command(dest: "A", comp: "M");
            this.WriteOutC_Command(dest: "M", comp: "D");
            this.WriteIncrementSP();
        }

        private void WritePopToA_Register()
        {
            this.WriteDecrementSP();
            this.WriteOutA_Command(VirtualRegisterConstants.SP);
            this.WriteOutC_Command(dest: "A", comp: "M");
        }

        #endregion

        #region SP (Stack Pointer) Operations

        private void WriteIncrementSP()
        {
            this.WriteOutA_Command(VirtualRegisterConstants.SP);
            this.WriteOutC_Command(dest: "M", comp: "M+1");           
        }

        private void WriteDecrementSP()
        {
            this.WriteOutA_Command(VirtualRegisterConstants.SP);
            this.WriteOutC_Command(dest: "M", comp: "M-1");           
        }

        #endregion

        #region Write out Hack commands

        private void WriteOutA_Command(string label)
        {
            this.assemblyFile.WriteLine(string.Concat("@", label));
        }

        private void WriteOutA_Command(int value)
        {
            this.assemblyFile.WriteLine($"@{value}");
        }

        private void WriteOutC_Command(string comp, string dest="",  string jump="")
        {
            Debug.Assert((dest != "" || jump != ""), "Invalid C command: dest and jump is empty.");

            if(jump == "")
            {
                this.assemblyFile.WriteLine($"{dest}={comp}");
            }
            else
            {
                this.assemblyFile.WriteLine($"{comp};{jump}");
            }
        }

        private void WriteOutL_Command(string label)
        {
            this.assemblyFile.WriteLine($"({label})");
        }

        private void WriteLine_Comment(string comment)
        {
            this.assemblyFile.WriteLine($"// {comment}");
        }

        #endregion

        #endregion

    }
}
