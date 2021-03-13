using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using JackCompiler.Interfaces;
using JackCompiler.Services;

namespace JackCompiler
{
    internal class JackCompilationEngine : ICompilationEngine
    {
        #region Fields

        private static int LabelCount = 0;

        private readonly SymbolTable symbolTable = new SymbolTable();
        private readonly IVmWriter vmWriter = new VmWriter();

        private string className;
        private IJackTokenNode jackTokenNode;

        #endregion

        #region Constructors

        internal JackCompilationEngine(string inputFilePath, string outputFilePath)
        {
            this.InitializeTokenizer(inputFilePath);
            this.CompileClass();
            this.SaveOutputs(outputFilePath);
        }

        #endregion

        #region Public Methods

        public void CompileClass()
        {
            this.CompileClassKeyword();
            this.className = this.CompileIdentifier();

            this.vmWriter.WriteLineComment($"class: {this.className}");

            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);

            if (this.jackTokenNode.IsCurrentTokenClassVarDecKeyword())
            {
                this.CompileClassVarDec();
            }

            if (this.jackTokenNode.IsCurrentTokenSubroutineDecKeyword())
            {
                this.CompileSubroutine();
            }

            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right);
        }

        public void CompileClassVarDec()
        {
            this.vmWriter.WriteLineComment("class variable declaration");

            var kind = this.CompileClassVarDecKeyword();
            var type = this.CompileTypeKeyword();
            var fieldVariableName = this.CompileIdentifier();

            this.symbolTable.Define(fieldVariableName, type, kind);

            while (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.SemiColon)
            {
                this.CompileSymbol(JackTokenConstants.Symbol.Comma);
                fieldVariableName = this.CompileIdentifier();

                this.symbolTable.Define(fieldVariableName, type, kind);
            }
                        
            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);

            if (this.jackTokenNode.IsCurrentTokenClassVarDecKeyword())
            {
                this.CompileClassVarDec();
            }
        }

        public void CompileSubroutine()
        {
            this.symbolTable.StartSubroutine();

            var subroutineDecKeyword = this.CompileSubroutineDecKeyword();
            if (subroutineDecKeyword == JackTokenConstants.Keyword.Method)
            {
                this.symbolTable.Define(VmConstants.Segment.This, this.className, AttributeType.Argument);
            }

            this.CompileSubroutineReturnType();
            var subroutineName = this.CompileIdentifier();

            this.vmWriter.WriteLineComment($"Subroutine: {subroutineName}");

            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Left);
            this.CompileParameterList();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
            this.CompileSubroutineBody($"{this.className}.{subroutineName}", subroutineDecKeyword);

            if (this.jackTokenNode.IsCurrentTokenSubroutineDecKeyword())
            {
                this.CompileSubroutine();
            }

        }

        public void CompileParameterList()
        {
            this.vmWriter.WriteLineComment("ParameterList");

            const AttributeType Kind = AttributeType.Argument;
            if (this.jackTokenNode.IsCurrentTokenTypeKeyword() == false 
                && this.jackTokenNode.Current.Type != JackTokenType.Identifier)
            {
                return;
            }

            var type = this.CompileTypeKeyword();
            var variableName = this.CompileIdentifier();

            this.symbolTable.Define(variableName, type, Kind);

            while (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.RoundBracket.Right)
            {
                this.CompileSymbol(JackTokenConstants.Symbol.Comma);

                type = this.CompileTypeKeyword();
                variableName = this.CompileIdentifier();

                this.symbolTable.Define(variableName, type, Kind);
            }
        }

        public void CompileVarDec(ref int numberOfLocals)
        {
            this.vmWriter.WriteLineComment("Variable Declaration");

            var kind = this.CompileVarDecKeyword();
            if (kind != AttributeType.Var)
            {
                return;
            }

            var type = this.CompileTypeKeyword();
            var variableName = this.CompileIdentifier();

            this.symbolTable.Define(variableName, type, kind);
            ++numberOfLocals;

            while (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.SemiColon)
            {
                this.CompileSymbol(JackTokenConstants.Symbol.Comma);

                variableName = this.CompileIdentifier();

                this.symbolTable.Define(variableName, type, kind);
                ++numberOfLocals;
            }

            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);

            if (this.jackTokenNode.IsCurrentTokenVarDecKeyword())
            {
                this.CompileVarDec(ref numberOfLocals);
            }

        }

        public void CompileStatements()
        {
            while (this.jackTokenNode.IsCurrentTokenFirstStatementWord())
            {
                var compileOneStatement = this.DetermineExecutingStatement();
                compileOneStatement();
            }
        }

        public void CompileDo()
        {
            this.vmWriter.WriteLineComment("Do statement");

            this.CompileFirstTokenInStatement();
            this.CompileSubroutineCall();
            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);

            this.vmWriter.WritePop(VmConstants.Segment.Temp, 0); // pop return value
        }

        public void CompileLet()
        {
            this.vmWriter.WriteLineComment("Let statement");

            this.CompileFirstTokenInStatement();
            var varName = this.CompileIdentifier();

            if (this.jackTokenNode.Current.Value == JackTokenConstants.Symbol.SquareBracket.Left)
            {
                this.WritePushVarName(varName);

                this.CompileSymbol(JackTokenConstants.Symbol.SquareBracket.Left);
                this.CompileExpression(); // index: i
                this.CompileSymbol(JackTokenConstants.Symbol.SquareBracket.Right);

                this.vmWriter.WriteArithmetic(VmConstants.Arithmetic.Add); // baseaddress + i

                this.CompileOperations(JackTokenConstants.Symbol.Equal);
                this.CompileExpression();

                this.vmWriter.WritePop(VmConstants.Segment.Temp, 0);    
                this.vmWriter.WritePop(VmConstants.Segment.Pointer, 1); 
                this.vmWriter.WritePush(VmConstants.Segment.Temp, 0);   
                this.vmWriter.WritePop(VmConstants.Segment.That, 0);
            }
            else
            {
                
                this.CompileOperations(JackTokenConstants.Symbol.Equal);
                this.CompileExpression();
                this.WritePopVarName(varName);
            }

            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);
        }

        public void CompileWhile()
        {
            this.vmWriter.WriteLineComment("While statement");

            var loopLabel = $"WhileLoop_{LabelCount}";
            var endLabel = $"WhileEnd_{LabelCount}";
            ++LabelCount;

            this.CompileFirstTokenInStatement();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Left);

            this.vmWriter.WriteLabel(loopLabel);
            this.CompileExpression();
            this.vmWriter.WriteArithmetic(VmConstants.Arithmetic.Not);

            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);

            this.vmWriter.WriteIf(endLabel);
            this.CompileStatements();
            this.vmWriter.WriteGoto(loopLabel);

            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right);

            this.vmWriter.WriteLabel(endLabel);
        }

        public void CompileReturn()
        {
            this.CompileFirstTokenInStatement();

            if (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.SemiColon)
            {
                this.CompileExpression();
            }
            else
            {
                const int VoidReturnValue = 0;
                this.vmWriter.WritePush(VmConstants.Segment.Constant, VoidReturnValue);
            }

            this.vmWriter.WriteReturn();

            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);
        }

        public void CompileIf()
        {
            this.vmWriter.WriteLineComment("If statement");

            var elseLabel =$"IfElse_{LabelCount}";
            var endLabel = $"IfEnd_{LabelCount}";
            ++LabelCount;

            this.CompileFirstTokenInStatement();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Left);

            this.CompileExpression();
            this.vmWriter.WriteArithmetic(VmConstants.Arithmetic.Not);
            this.vmWriter.WriteIf(elseLabel);

            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);

            this.CompileStatements();

            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right);

            this.vmWriter.WriteGoto(endLabel);
            this.vmWriter.WriteLabel(elseLabel);
            this.CompileElse();

            this.vmWriter.WriteLabel(endLabel);
        }

        public void CompileExpression()
        {
            this.CompileTerm();

            while (this.jackTokenNode.IsCurrentTokenOperation())
            {
                var jackOperator = this.jackTokenNode.Current.Value;
                this.jackTokenNode.AdvanceToken();

                this.CompileTerm();

                this.WriteBinaryOperations(jackOperator);
            }
        }

        public void CompileTerm()
        {
            if (this.jackTokenNode.Current.Type == JackTokenType.IntegerConstant)
            {
                var intConst = int.Parse(this.jackTokenNode.Current.Value);
                this.vmWriter.WritePush(VmConstants.Segment.Constant, intConst);

                this.jackTokenNode.AdvanceToken();
                return;
            }

            if (this.jackTokenNode.Current.Type == JackTokenType.StringConstant)
            {
                var stringConst = this.jackTokenNode.Current.Value;
                var stringLength = stringConst.Length;
                this.vmWriter.WritePush(VmConstants.Segment.Constant, stringLength);
                this.vmWriter.WriteCall(@"String.new", 1);

                foreach (var c in stringConst)
                {
                    this.vmWriter.WritePush(VmConstants.Segment.Constant, Convert.ToInt32(c));
                    this.vmWriter.WriteCall(@"String.appendChar", 2);
                }

                this.jackTokenNode.AdvanceToken();
                return;
            }

            if (this.jackTokenNode.IsCurrentTokenKeywordConstant())
            {
                var keywordConst = this.jackTokenNode.Current.Value;
                if (VmConstants.KeywordConstantsVMValues.ContainsKey(keywordConst))
                {
                    if (keywordConst == JackTokenConstants.Keyword.True)
                    {
                        this.vmWriter.WritePush(VmConstants.Segment.Constant, 1);
                        this.vmWriter.WriteArithmetic(VmConstants.Arithmetic.Negation);
                    }
                    else
                    {
                        var vmConstantValue = VmConstants.KeywordConstantsVMValues[keywordConst];
                        this.vmWriter.WritePush(VmConstants.Segment.Constant, vmConstantValue);
                    }
                }
                else
                {
                    this.vmWriter.WritePush(VmConstants.Segment.Pointer, 0);
                }

                this.jackTokenNode.AdvanceToken();
                return;
            }

            if (this.jackTokenNode.IsCurrentTokenUnaryOperation())
            {
                var unaryOperation = VmConstants.JackVmUnaryOperationTable[this.jackTokenNode.Current.Value];
                this.jackTokenNode.AdvanceToken();

                this.CompileTerm();
                this.vmWriter.WriteArithmetic(unaryOperation);
                return;
            }

            if (this.jackTokenNode.Current.Value == JackTokenConstants.Symbol.RoundBracket.Left)
            {
                this.jackTokenNode.AdvanceToken();
                this.CompileExpression();
                this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
                return;
            }

            if (this.jackTokenNode.Next.Value == JackTokenConstants.Symbol.RoundBracket.Left
                || this.jackTokenNode.Next.Value == JackTokenConstants.Symbol.Period)
            {
                this.CompileSubroutineCall();
                return;
            }

            // varName | varName '[' expression ']'
            if (this.jackTokenNode.Current.Type == JackTokenType.Identifier)
            {
                var varName = this.CompileIdentifier();
                this.WritePushVarName(varName);
                if (this.jackTokenNode.Current.Value == JackTokenConstants.Symbol.SquareBracket.Left)
                {
                    this.CompileSymbol(JackTokenConstants.Symbol.SquareBracket.Left);

                    this.CompileExpression();
                    this.vmWriter.WriteArithmetic(VmConstants.Arithmetic.Add);
                    this.vmWriter.WritePop(VmConstants.Segment.Pointer, 1);
                    this.vmWriter.WritePush(VmConstants.Segment.That, 0);

                    this.CompileSymbol(JackTokenConstants.Symbol.SquareBracket.Right);
                    return;
                }
            }
        }

        public int CompileExpressionList()
        {
            int numberOfArguments = 0;

            if (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.RoundBracket.Right
                && this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.SquareBracket.Right)
            {
                this.CompileExpression();
                ++numberOfArguments;

                while (this.jackTokenNode.Current.Value == JackTokenConstants.Symbol.Comma)
                {
                    this.jackTokenNode.AdvanceToken();
                    this.CompileExpression();
                    ++numberOfArguments;
                }
            }

            return numberOfArguments;

        }

        #endregion

        #region Private Methods

        private void InitializeTokenizer(string inputFilePath)
        {
            this.jackTokenNode = new JackTokenNode(new JackTokenizer(inputFilePath));

            if (this.jackTokenNode.ExistsNextToken == false)
            {
                Console.Error.WriteLine($"Not Found any token. filepath: {inputFilePath}");
            }
        }

        private void SaveOutputs(string outputFilePath)
        {
            File.WriteAllLines(outputFilePath, this.vmWriter.VmCodeWriteLines);
        }

        #region Keywords

        private void CompileClassKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenClassKeyword() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return;
            }

            this.jackTokenNode.AdvanceToken();
        }

        private AttributeType CompileClassVarDecKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenClassVarDecKeyword() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return AttributeType.None;
            }
            var kind = this.jackTokenNode.Current.Value;

            this.jackTokenNode.AdvanceToken();

            return this.symbolTable.ConvertStringKindToAttributeType(kind);
        }

        private string CompileTypeKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenTypeKeyword())
            {
                var type = this.jackTokenNode.Current.Value;
                this.jackTokenNode.AdvanceToken();
                return type;
            }

            if (this.jackTokenNode.Current.Type != JackTokenType.Identifier)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return null;
            }

            return this.CompileIdentifier();
        }

        private AttributeType CompileVarDecKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenVarDecKeyword() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return AttributeType.None;
            }

            var kind = this.jackTokenNode.Current.Value;

            this.jackTokenNode.AdvanceToken();

            return this.symbolTable.ConvertStringKindToAttributeType(kind);
        }

        #endregion

        #region Symbol

        private void CompileSymbol(string targetSymbol)
        {
            if (this.jackTokenNode.Current.Value != targetSymbol)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return;
            }

            this.jackTokenNode.AdvanceToken();
        }

        #endregion

        #region Identifier

        private string CompileIdentifier()
        {
            if (this.jackTokenNode.Current.Type != JackTokenType.Identifier)
            {
                Debug.WriteLine($"Next token is not identifier. {this.jackTokenNode.Current.Value}");
                return null;
            }

            var identifier = this.jackTokenNode.Current.Value;

            this.jackTokenNode.AdvanceToken();

            return identifier;
        }
        #endregion

        #region Subroutines

        private string CompileSubroutineDecKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenSubroutineDecKeyword() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return null;
            }

            var decKeyword = this.jackTokenNode.Current.Value;

            this.jackTokenNode.AdvanceToken();

            return decKeyword;
        }

        private void CompileSubroutineReturnType()
        {
            if (this.jackTokenNode.Current.Value == JackTokenConstants.Keyword.Void)
            {
                this.jackTokenNode.AdvanceToken();
                return;
            }

            this.CompileTypeKeyword();
        }

        private void CompileSubroutineBody(string subroutineFullName, string subroutineDecKeyword)
        {
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);

            int numberOfLocals = 0;
            if (this.jackTokenNode.IsCurrentTokenVarDecKeyword())
            {
                this.CompileVarDec(ref numberOfLocals);
            }

            this.vmWriter.WriteFunction(subroutineFullName, numberOfLocals);
            if (subroutineDecKeyword == JackTokenConstants.Keyword.Constructor)
            {
                this.vmWriter.WritePush(VmConstants.Segment.Constant, this.symbolTable.VarCount(AttributeType.Field));
                this.vmWriter.WriteCall("Memory.alloc", 1);
                this.vmWriter.WritePop(VmConstants.Segment.Pointer, 0);
            }
            else if (subroutineDecKeyword == JackTokenConstants.Keyword.Method)
            {
                this.vmWriter.WritePush(VmConstants.Segment.Argument, 0);
                this.vmWriter.WritePop(VmConstants.Segment.Pointer, 0);
            }

            this.CompileStatements();
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right);
        }

        private void CompileSubroutineCall()
        {
            var subroutineName = this.CompileIdentifier();

            if (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.Period)
            {
                this.vmWriter.WritePush(VmConstants.Segment.Pointer, 0);
                this.vmWriter.WriteCall($"{this.className}.{subroutineName}", GetNumberOfArguments("method"));
                return;
            }

            this.CompileSymbol(JackTokenConstants.Symbol.Period);

            var classVarName = subroutineName;
            subroutineName = this.CompileIdentifier();

            var classVarKind = this.symbolTable.KindOf(classVarName);
            if (classVarKind == AttributeType.None)  // className.Subroutine()
            {
                this.vmWriter.WriteCall($"{classVarName}.{subroutineName}", GetNumberOfArguments("function"));
            }
            else
            {
                this.WritePushVarName(classVarName);
                var type = this.symbolTable.TypeOf(classVarName);
                this.vmWriter.WriteCall($"{type}.{subroutineName}", GetNumberOfArguments("method"));
            }

            int GetNumberOfArguments(string callingType)
            {
                const string typeMethod = "method";
                const string typeFunction = "function";

                int nArgs;
                if(Regex.IsMatch(callingType, typeFunction, RegexOptions.IgnoreCase))
                {
                    nArgs = 0;
                }
                else if (Regex.IsMatch(callingType, typeMethod, RegexOptions.IgnoreCase))
                {
                    nArgs = 1; // メソッドの属するオブジェクト参照分
                }
                else
                {
                    Debug.Assert(false, "Unknown type. GetNumberOfArguments()");
                    return -1;
                }

                this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Left);
                nArgs += this.CompileExpressionList();
                this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);

                return nArgs;
            }

        }

        #endregion

        #region Statements

        private void CompileFirstTokenInStatement()
        {
            if (this.jackTokenNode.IsCurrentTokenFirstStatementWord() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return;
            }

            this.jackTokenNode.AdvanceToken();
        }

        private void CompileElse()
        {
            if (this.jackTokenNode.Current.Value != JackTokenConstants.Keyword.Else)
            {
                return;
            }

            this.jackTokenNode.AdvanceToken();
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);
            this.CompileStatements();
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right);
        }

        #endregion

        #region Expressions

        private void CompileOperations(string targetOperation)
        {
            if (this.jackTokenNode.IsCurrentTokenOperation() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return;
            }

            this.CompileSymbol(targetOperation);
        }

        private Action DetermineExecutingStatement()
        {
            switch (this.jackTokenNode.Current.Value)
            {
                case JackTokenConstants.Keyword.Let:
                    return this.CompileLet;

                case JackTokenConstants.Keyword.If:
                    return this.CompileIf;

                case JackTokenConstants.Keyword.While:
                    return this.CompileWhile;

                case JackTokenConstants.Keyword.Do:
                    return this.CompileDo;

                case JackTokenConstants.Keyword.Return:
                    return this.CompileReturn;

                default:
                    return null;
            }
        }

        #endregion

        private void WritePushVarName(string varName)
        {
            var kind = this.symbolTable.KindOf(varName);
            var index = this.symbolTable.IndexOf(varName);

            switch (kind)
            {
                case AttributeType.Argument:
                    this.vmWriter.WritePush(VmConstants.Segment.Argument, index);
                    break;

                case AttributeType.Var:
                    this.vmWriter.WritePush(VmConstants.Segment.Local, index);
                    break;

                case AttributeType.Field:
                    this.vmWriter.WritePush(VmConstants.Segment.This, index);
                    break;

                case AttributeType.Static:
                    this.vmWriter.WritePush(VmConstants.Segment.Static, index);
                    break;
            }
        }

        private void WritePopVarName(string varName)
        {
            var kind = this.symbolTable.KindOf(varName);
            var index = this.symbolTable.IndexOf(varName);

            switch (kind)
            {
                case AttributeType.Argument:
                    this.vmWriter.WritePop(VmConstants.Segment.Argument, index);
                    break;

                case AttributeType.Var:
                    this.vmWriter.WritePop(VmConstants.Segment.Local, index);
                    break;

                case AttributeType.Field:
                    this.vmWriter.WritePop(VmConstants.Segment.This, index);
                    break;

                case AttributeType.Static:
                    this.vmWriter.WritePop(VmConstants.Segment.Static, index);
                    break;
            }
        }

        private void WriteBinaryOperations(string @operator)
        {
            switch (@operator)
            {
                case JackTokenConstants.Symbol.Addition:
                case JackTokenConstants.Symbol.Subtraction:
                case JackTokenConstants.Symbol.Equal:
                case JackTokenConstants.Symbol.Greater:
                case JackTokenConstants.Symbol.Less:
                case JackTokenConstants.Symbol.And:
                case JackTokenConstants.Symbol.Or:
                    this.vmWriter.WriteArithmetic(VmConstants.JackVmBinaryOperationTable[@operator]);
                    return;
                case JackTokenConstants.Symbol.Multiplication:
                    this.vmWriter.WriteCall(@"Math.multiply", 2);
                    return;
                case JackTokenConstants.Symbol.Division:
                    this.vmWriter.WriteCall(@"Math.divide", 2);
                    return;
            }
        }

        #endregion
    }
}