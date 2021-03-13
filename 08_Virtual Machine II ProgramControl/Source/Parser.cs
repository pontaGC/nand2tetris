using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VMTranslation
{
    public class Parser
    {
        #region Fields

        private static readonly Dictionary<string, VMCommandType> VMCommandTypeTable = new Dictionary<string, VMCommandType>()
        {
            { VMCommandConstants.Add,         VMCommandType.C_ARITHMETIC },
            { VMCommandConstants.Subtruction, VMCommandType.C_ARITHMETIC },
            { VMCommandConstants.Negation,    VMCommandType.C_ARITHMETIC },  
            { VMCommandConstants.Equality,    VMCommandType.C_ARITHMETIC },   
            { VMCommandConstants.Greater,     VMCommandType.C_ARITHMETIC }, 
            { VMCommandConstants.Less,        VMCommandType.C_ARITHMETIC },
            { VMCommandConstants.And,         VMCommandType.C_ARITHMETIC },   
            { VMCommandConstants.Or,          VMCommandType.C_ARITHMETIC },   
            { VMCommandConstants.Not,         VMCommandType.C_ARITHMETIC },   
            { VMCommandConstants.Push,        VMCommandType.C_PUSH },
            { VMCommandConstants.Pop,         VMCommandType.C_POP },
            { VMCommandConstants.Label,       VMCommandType.C_LABEL },
            { VMCommandConstants.Goto,        VMCommandType.C_GOTO },
            { VMCommandConstants.If,          VMCommandType.C_IF },
            { VMCommandConstants.Function,    VMCommandType.C_FUNCTION },
            { VMCommandConstants.Return,      VMCommandType.C_RETURN },
            { VMCommandConstants.Call,        VMCommandType.C_CALL },
        };

        private string[] currentInstruction;
        private Queue<string> vmInstructions = new Queue<string>();

        #endregion

        #region Constructors

        public Parser(string vmFilePath)
        {
            var allCommandLines = File.ReadAllLines(vmFilePath).Where(l => Regex.IsMatch(l, "^//.*|^$") == false);
            this.vmInstructions = new Queue<string>(allCommandLines);            
        }

        #endregion

        #region Properies

        public string CurrentCommand
        {
            get => this.currentInstruction?.First();
        }

        #endregion

        #region Public Methods

        public bool HasMoreCommands()
        {
            return this.vmInstructions.Any();
        }

        public void Advance()
        {
            this.currentInstruction = this.vmInstructions.Dequeue().Split(' ');
        }

        public VMCommandType CommandType()
        {
            return VMCommandTypeTable[this.CurrentCommand];
        } 

        public string Arg1()
        {
           if (this.CommandType() == VMCommandType.C_ARITHMETIC)
           {
                return this.CurrentCommand;
           }

            return this.currentInstruction.Skip(1).First();
        }

        public int Arg2()
        {           
            return int.Parse(this.currentInstruction.Skip(2).First());
        }

        #endregion

    }
}
