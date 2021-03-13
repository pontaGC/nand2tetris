using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HackAssemblerCompiler
{
    public class Program
    {
        private const int CommandBitNumbers = 16;
     
        private static void Main()
        {
            const string AssemblyFile = @"./06/pong/Pong.asm";            

            if (File.Exists(AssemblyFile))
            {
                var compiledFile = $"{Path.GetDirectoryName(AssemblyFile)}\\{Path.GetFileNameWithoutExtension(AssemblyFile)}.hack";
                File.WriteAllLines(compiledFile, CompileAssemblyCodes(AssemblyFile));
            }
            else
            {
                throw new FileNotFoundException($"Not found {AssemblyFile}.");                
            }
        }

        #region Compile Commands

        private static IEnumerable<string> CompileAssemblyCodes(string assemblyFile)
        {
            var readAllLines = File.ReadAllLines(assemblyFile);

            var symbolTable = CreateSymbolTable(readAllLines);

            var binaryCodes = new List<string>();
            var parser = new Parser(readAllLines);
            while (parser.HasMoreCommands())
            {
                parser.Advance();

                var commandType = parser.CommandType();
                if (commandType == HackCommandType.A_Command)
                {
                    binaryCodes.Add(CompileA_Command(parser.Symbol(), symbolTable));
                }
                else if (commandType == HackCommandType.L_Command)
                {
                    binaryCodes.Add(CompileL_Command(parser.Symbol(), symbolTable));
                }
                else if (commandType == HackCommandType.C_Command)
                {
                    binaryCodes.Add(CompileC_Comand(parser.Dest(), parser.Comp(), parser.Jump()));
                }

            }

            return binaryCodes;
        }

        private static string CompileA_Command(string symbol, SymbolTable symbolTable)
        {
            if (IsValueSymbol(symbol))
            {
                return Convert.ToString(int.Parse(symbol), 2).PadLeft(CommandBitNumbers, '0');
            }
            else
            {
                return Convert.ToString(symbolTable.GetAddress(symbol), 2).PadLeft(CommandBitNumbers, '0');
            }
        }

        private static string CompileL_Command(string symbol, SymbolTable symbolTable)
        {
            return Convert.ToString(symbolTable.GetAddress(symbol), 2).PadLeft(CommandBitNumbers, '0');
        }

        private static string CompileC_Comand(string destSymbol, string compSymbol, string jumpSymbol)
        {
            return $"{Code.Comp(compSymbol)}{Code.Dest(destSymbol)}{Code.Jump(jumpSymbol)}".PadLeft(CommandBitNumbers, '1');
        }

        #endregion

        #region Create Symbol Table

        private static SymbolTable CreateSymbolTable(IEnumerable<string> assemblyFileAllLines)
        {
            var symbolTable = new SymbolTable();

            RegisterPseudoSymbol(assemblyFileAllLines, symbolTable);
            RegisterVariableSymbol(assemblyFileAllLines, symbolTable);

            return symbolTable;
        }

        private static void RegisterPseudoSymbol(IEnumerable<string> assemblyFileAllLines, SymbolTable symbolTable)
        {
            var parser = new Parser(assemblyFileAllLines);

            int programCount = 0;
            while (parser.HasMoreCommands())
            {
                parser.Advance();

                if(parser.CommandType() == HackCommandType.L_Command)
                {
                    var pseudoSymbol = parser.Symbol();
                    if(symbolTable.Contains(pseudoSymbol) == false)
                    {
                        symbolTable.AddEntry(pseudoSymbol, programCount);
                    }
                }
                else
                {
                    ++programCount;
                }
            }
        }

        private static void RegisterVariableSymbol(IEnumerable<string> assemblyFileAllLines, SymbolTable symbolTable)
        {
            var parser = new Parser(assemblyFileAllLines);

            while (parser.HasMoreCommands())
            {
                parser.Advance();

                if (parser.CommandType() == HackCommandType.A_Command)
                {
                    var variableSymbol = parser.Symbol();
                    if (IsValueSymbol(variableSymbol) == false && symbolTable.Contains(variableSymbol) == false)
                    {
                        symbolTable.AddEntry(variableSymbol, symbolTable.NextAddress);
                        ++symbolTable.NextAddress;
                    }
                }
            }
        }

        #endregion

        #region Check Symbol

        private static bool IsValueSymbol(string targetSymbol)
        {
            return Regex.IsMatch(targetSymbol, "^[0-9]+");
        }

        #endregion
    }
}