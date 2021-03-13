using System.Collections.Generic;

namespace HackAssemblerCompiler
{
    public class Code
    {
        #region Fields

        private static readonly Dictionary<string, string> DestBinaryTable = new Dictionary<string, string>()
        {
            { "",     @"000" },
            { @"M",   @"001" },
            { @"D",   @"010" },
            { @"MD",  @"011" },
            { @"A",   @"100" },
            { @"AM",  @"101" },
            { @"AD",  @"110" },
            { @"AMD", @"111" },
        };

        private static readonly Dictionary<string, string> CompBinaryTableA0 = new Dictionary<string, string>()
        {
            { @"0",   @"101010" },
            { @"1",   @"111111" },
            { @"-1",  @"111010" },
            { @"D",   @"001100" },
            { @"A",   @"110000" },
            { @"!D",  @"001101" },
            { @"!A",  @"110001" },
            { @"-D",  @"001111" },
            { @"-A",  @"110011" },
            { @"D+1", @"011111" },
            { @"A+1", @"110111" },
            { @"D-1", @"001110" },
            { @"A-1", @"110010" },
            { @"D+A", @"000010" },
            { @"D-A", @"010011" },
            { @"A-D", @"000111" },
            { @"D&A", @"000000" },
            { @"D|A", @"010101" },

        };

        private static readonly Dictionary<string, string> CompBinaryTableA1 = new Dictionary<string, string>()
        {

           { @"M",   @"110000" },
           { @"!M",  @"110001" },
           { @"-M",  @"110011" },
           { @"M+1", @"110111" },
           { @"M-1", @"110010" },
           { @"D+M", @"000010" },
           { @"D-M", @"010011" },
           { @"M-D", @"000111" },
           { @"D&M", @"000000" },
           { @"D|M", @"010101" },
        };

        private static readonly Dictionary<string, string> JumpBinaryTable = new Dictionary<string, string>()
        {
            { "",     @"000" },
            { @"JGT", @"001" },
            { @"JEQ", @"010" },
            { @"JGE", @"011" },
            { @"JLT", @"100" },
            { @"JNE", @"101" },
            { @"JLE", @"110" },
            { @"JMP", @"111" },
        };

        #endregion

        #region Public Methods

        public static string Dest(string symbol)
        {
            return DestBinaryTable.TryGetValue(symbol, out var binary) ? binary : null;
        }

        public static string Comp(string symbol)
        {
            if(CompBinaryTableA0.TryGetValue(symbol, out var binary0))
            {
                return string.Concat("0", binary0);
            }
            else if(CompBinaryTableA1.TryGetValue(symbol, out var binary1))
            {
                return string.Concat("1", binary1);
            }

            return null;
        }

        public static string Jump(string symbol)
        {
            return JumpBinaryTable.TryGetValue(symbol, out var binary) ? binary : null;
        }

        #endregion
    }
}
