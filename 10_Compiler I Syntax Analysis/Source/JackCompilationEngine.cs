using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using JackCompiler.Interfaces;
using JackCompiler.Services;

namespace JackCompiler
{
    internal class JackCompilationEngine : ICompilationEngine
    {
        #region Fields

        private IJackTokenNode jackTokenNode;
        private readonly ICompilationXmlWriter xmlWriter = new JackCompilationXmlWriter();

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
            this.StartSyntaxCompilation(JackSyntaxType.Class);

            this.CompileClassKeyword();
            this.CompileIdentifier(); // className
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);

            if (this.jackTokenNode.IsCurrentTokenClassVarDecKeyword())
            {
                this.CompileClassVarDec();
            }

            if (this.jackTokenNode.IsCurrentTokenSubroutineDecKeyword())
            {
                this.CompileSubroutine();
            }

            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right); // }

            this.FinishSyntaxCompilation(JackSyntaxType.Class);
        }

        public void CompileClassVarDec()
        {
            this.StartSyntaxCompilation(JackSyntaxType.ClassVarDec);

            this.CompileClassVarDecKeyword();
            this.CompileTypeKeyword(); 
            this.CompileIdentifier(); // varName

            while (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.SemiColon)
            {
                this.CompileSymbol(JackTokenConstants.Symbol.Comma);
                this.CompileIdentifier(); // varName
            }

            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);

            this.FinishSyntaxCompilation(JackSyntaxType.ClassVarDec);

            if (this.jackTokenNode.IsCurrentTokenClassVarDecKeyword())
            {
                this.CompileClassVarDec();
            }
        }

        public void CompileSubroutine()
        {
            this.StartSyntaxCompilation(JackSyntaxType.SubroutineDec);

            this.CompileSubroutineDecKeyword();
            this.CompileSubroutineReturnType();
            this.CompileIdentifier(); // subroutineName
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Left);
            this.CompileParameterList();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
            this.CompileSubroutineBody();

            this.FinishSyntaxCompilation(JackSyntaxType.SubroutineDec);

            if (this.jackTokenNode.IsCurrentTokenSubroutineDecKeyword())
            {
                this.CompileSubroutine();
            }

        }

        public void CompileParameterList()
        {
            this.StartSyntaxCompilation(JackSyntaxType.ParameterList);

            if (this.jackTokenNode.IsCurrentTokenTypeKeyword() == false)
            {
                this.FinishSyntaxCompilation(JackSyntaxType.ParameterList);
                return;
            }

            this.CompileTypeKeyword();
            this.CompileIdentifier(); // varName

            while (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.RoundBracket.Right)
            {
                this.CompileSymbol(JackTokenConstants.Symbol.Comma);
                this.CompileTypeKeyword();
                this.CompileIdentifier(); // varName
            }

            this.FinishSyntaxCompilation(JackSyntaxType.ParameterList);
        }

        public void CompileVarDec()
        {
            this.StartSyntaxCompilation(JackSyntaxType.VarDec);

            this.CompileVarDecKeyword();
            this.CompileTypeKeyword();
            this.CompileTypeKeyword();

            while (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.SemiColon)
            {
                this.CompileSymbol(JackTokenConstants.Symbol.Comma);
                this.CompileIdentifier(); // varName
            }

            this.WriteLineAndAdvanceToken();

            this.FinishSyntaxCompilation(JackSyntaxType.VarDec);

            if (this.jackTokenNode.IsCurrentTokenVarDecKeyword())
            {
                this.CompileVarDec();
            }
        }

        public void CompileStatements()
        {
            this.StartSyntaxCompilation(JackSyntaxType.Statements);

            while (this.jackTokenNode.IsCurrentTokenFirstStatementWord())
            {
                var compileOneStatement = this.DetermineExecutingStatement();
                compileOneStatement();
            }

            this.FinishSyntaxCompilation(JackSyntaxType.Statements);
        }

        public void CompileDo()
        {
            this.StartSyntaxCompilation(JackSyntaxType.Do);

            this.CompileFirstTokenInStatement();
            this.CompileSubroutineCall();
            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);

            this.FinishSyntaxCompilation(JackSyntaxType.Do);
        }

        public void CompileLet()
        {
            this.StartSyntaxCompilation(JackSyntaxType.Let);

            this.CompileFirstTokenInStatement();
            this.CompileIdentifier(); // varName

            if (this.jackTokenNode.Current.Value == JackTokenConstants.Symbol.SquareBracket.Left)
            {
                this.WriteLineAndAdvanceToken();
                this.CompileExpression();
                this.CompileSymbol(JackTokenConstants.Symbol.SquareBracket.Right);
            }

            this.CompileOperations(JackTokenConstants.Symbol.Equal);
            this.CompileExpression();
            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);

            this.FinishSyntaxCompilation(JackSyntaxType.Let);
        }

        public void CompileWhile()
        {

            this.StartSyntaxCompilation(JackSyntaxType.While);

            this.CompileFirstTokenInStatement();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Left);
            this.CompileExpression();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);
            this.CompileStatements();
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right);

            this.FinishSyntaxCompilation(JackSyntaxType.While);
        }

        public void CompileReturn()
        {
            this.StartSyntaxCompilation(JackSyntaxType.Return);

            this.CompileFirstTokenInStatement();

            if (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.SemiColon)
            {
                this.CompileExpression();
            }

            this.CompileSymbol(JackTokenConstants.Symbol.SemiColon);

            this.FinishSyntaxCompilation(JackSyntaxType.Return);
        }

        public void CompileIf()
        {
            this.StartSyntaxCompilation(JackSyntaxType.If);

            this.CompileFirstTokenInStatement();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Left);
            this.CompileExpression();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);
            this.CompileStatements();
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right);
            this.CompileElse();

            this.FinishSyntaxCompilation(JackSyntaxType.If);
        }

        public void CompileExpression()
        {
            this.StartSyntaxCompilation(JackSyntaxType.Expression);
            this.CompileTerm();
            while (this.jackTokenNode.IsCurrentTokenOperation())
            {
                this.WriteLineAndAdvanceToken();
                this.CompileTerm();
            }
            this.FinishSyntaxCompilation(JackSyntaxType.Expression);
        }

        public void CompileTerm()
        {
            if (this.jackTokenNode.Current.Type == JackTokenType.IntegerConstant
                || this.jackTokenNode.Current.Type == JackTokenType.StringConstant
                || this.jackTokenNode.IsCurrentTokenKeywordConstant())
            {
                WriteTermAndAdvance();
                return;
            }

            if (this.jackTokenNode.IsCurrentTokenUnaryOperation())
            {
                this.StartSyntaxCompilation(JackSyntaxType.Term);
                this.WriteLineAndAdvanceToken();
                this.CompileTerm();
                this.FinishSyntaxCompilation(JackSyntaxType.Term);
                return;
            }

            if (this.jackTokenNode.Current.Value == JackTokenConstants.Symbol.RoundBracket.Left)
            {
                
                this.StartSyntaxCompilation(JackSyntaxType.Term);
                this.WriteLineAndAdvanceToken();
                this.CompileExpression();
                this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
                this.FinishSyntaxCompilation(JackSyntaxType.Term);
                return;
            }

            // varName '[' expression ']'
            if (this.jackTokenNode.Next.Value == JackTokenConstants.Symbol.SquareBracket.Left)
            {
                this.StartSyntaxCompilation(JackSyntaxType.Term);
                this.WriteLineAndAdvanceToken();
                this.CompileSymbol(JackTokenConstants.Symbol.SquareBracket.Left);
                this.CompileExpression();
                this.CompileSymbol(JackTokenConstants.Symbol.SquareBracket.Right);
                this.FinishSyntaxCompilation(JackSyntaxType.Term);
                return;
            }

            // SubroutineCall
            if (this.jackTokenNode.Next.Value == JackTokenConstants.Symbol.RoundBracket.Left
                || this.jackTokenNode.Next.Value == JackTokenConstants.Symbol.Period)
            {
                this.StartSyntaxCompilation(JackSyntaxType.Term);
                this.CompileSubroutineCall();
                this.FinishSyntaxCompilation(JackSyntaxType.Term);
                return;
            }

            // varName
            WriteTermAndAdvance();

            void WriteTermAndAdvance()
            {
                this.StartSyntaxCompilation(JackSyntaxType.Term);
                this.WriteLineAndAdvanceToken();
                this.FinishSyntaxCompilation(JackSyntaxType.Term);
            }
        }

        public void CompileExpressionList()
        {
            this.StartSyntaxCompilation(JackSyntaxType.ExpressionList);

            if (this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.RoundBracket.Right
                && this.jackTokenNode.Current.Value != JackTokenConstants.Symbol.SquareBracket.Right)
            {
                this.CompileExpression();
                while (this.jackTokenNode.Current.Value == JackTokenConstants.Symbol.Comma)
                {
                    this.WriteLineAndAdvanceToken();
                    this.CompileExpression();
                }
            }

            this.FinishSyntaxCompilation(JackSyntaxType.ExpressionList);
        }

        #endregion

        #region Private Methods

        private void StartSyntaxCompilation(JackSyntaxType syntaxType)
        {
            this.xmlWriter.WriteLineSyntaxTag(syntaxType, XmlTagTypes.StartTag);
        }

        private void FinishSyntaxCompilation(JackSyntaxType syntaxType)
        {
            this.xmlWriter.WriteLineSyntaxTag(syntaxType, XmlTagTypes.EndTag);
        }

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
            File.WriteAllLines(outputFilePath, this.xmlWriter.WriteLines);
        }

        private void WriteLineAndAdvanceToken()
        {
            this.xmlWriter.WriteLineTokenTag(this.jackTokenNode.Current);
            this.jackTokenNode.AdvanceToken();
        }

        #region Keywords

        private void CompileClassKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenClassKeyword() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return;
            }

            this.WriteLineAndAdvanceToken();
        }

        private void CompileClassVarDecKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenClassVarDecKeyword() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return;
            }

            this.WriteLineAndAdvanceToken();
        }

        private void CompileTypeKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenTypeKeyword())
            {
                this.WriteLineAndAdvanceToken();
                return;
            }

            if (this.jackTokenNode.Current.Type != JackTokenType.Identifier)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
            }

            this.CompileIdentifier();
        }

        private void CompileVarDecKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenVarDecKeyword() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return;
            }

            this.WriteLineAndAdvanceToken();
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

            this.WriteLineAndAdvanceToken();
        }

        #endregion

        #region Identifier

        private void CompileIdentifier()
        {
            if (this.jackTokenNode.Current.Type != JackTokenType.Identifier)
            {
                Debug.WriteLine($"Next token is not identifier. {this.jackTokenNode.Current.Value}");
                return;
            }

            this.WriteLineAndAdvanceToken();
        }

        #endregion

        #region Subroutines

        private void CompileSubroutineDecKeyword()
        {
            if (this.jackTokenNode.IsCurrentTokenSubroutineDecKeyword() == false)
            {
                Debug.WriteLine($"{MethodBase.GetCurrentMethod().Name}/{this.jackTokenNode.Current.Value}");
                return;
            }

            this.WriteLineAndAdvanceToken();
        }

        private void CompileSubroutineReturnType()
        {
            if (this.jackTokenNode.Current.Value == JackTokenConstants.Keyword.Void)
            {
                this.WriteLineAndAdvanceToken();
                return;
            }

            this.CompileTypeKeyword();
        }

        private void CompileSubroutineBody()
        {
            this.StartSyntaxCompilation(JackSyntaxType.SubroutineBody);

            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Left);

            if (this.jackTokenNode.IsCurrentTokenVarDecKeyword())
            {
                this.CompileVarDec();
            }

            this.CompileStatements();
            this.CompileSymbol(JackTokenConstants.Symbol.CurlyBracket.Right);

            this.FinishSyntaxCompilation(JackSyntaxType.SubroutineBody);
        }

        private void CompileSubroutineCall()
        {
            this.CompileIdentifier(); // subroutineName | (className | varName)

            if (this.jackTokenNode.Current.Value == JackTokenConstants.Symbol.Period)
            {
                this.CompileSymbol(JackTokenConstants.Symbol.Period);
                this.CompileIdentifier();// subroutineName
            }

            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Left);
            this.CompileExpressionList();
            this.CompileSymbol(JackTokenConstants.Symbol.RoundBracket.Right);
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

            this.WriteLineAndAdvanceToken();
        }

        private void CompileElse()
        {
            if (this.jackTokenNode.Current.Value != JackTokenConstants.Keyword.Else)
            {
                return;
            }

            this.WriteLineAndAdvanceToken();
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

        #endregion
    }
}