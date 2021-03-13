using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HackAssemblerCompiler
{
    public class Parser
    {
        #region Fields

        private const string SymbolPattern = "([0-9a-zA-Z_\\.\\$:]+)";
        private const string C_CommandPattern = "(([ADM]+)=)?([!ADM01\\-\\+\\|&]+)(;(J[EGLMN][EPQT]))?";
        private Queue<string> assemblyCommands;
        private string currentCommand = null;

        #endregion

        #region Constructors

        public Parser(IEnumerable<string> assemblyFileAllLines)
        {
            var allCommands = assemblyFileAllLines.Where(l => Regex.IsMatch(l, "^//.*|^$") == false);
            this.assemblyCommands = new Queue<string>(allCommands);
        }

        #endregion

        #region Public Methods

        public bool HasMoreCommands()
        {
            return this.assemblyCommands.Any();
        }
        public void Advance()
        {
            this.currentCommand = this.assemblyCommands.Dequeue();
        }

        public HackCommandType CommandType()
        {
            switch (this.currentCommand.First())
            {
                case '@':
                    return HackCommandType.A_Command;
                case '(':
                    return HackCommandType.L_Command;
                default:
                    return HackCommandType.C_Command;
            }
        }

        public string Symbol()
        {
            return Regex.Match(this.currentCommand, SymbolPattern).Value;
        }

        public string Dest()
        {
            return Regex.Match(this.currentCommand, C_CommandPattern).Groups[2].Value;
        }

        public string Comp()
        {
            return Regex.Match(this.currentCommand, C_CommandPattern).Groups[3].Value; 
        }

        public string Jump()
        {
            return Regex.Match(this.currentCommand, C_CommandPattern).Groups[5].Value;
        }

        #endregion

    }
}

