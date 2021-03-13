using System.Diagnostics;

using JackCompiler.Interfaces;

namespace JackCompiler.Services
{
    internal class JackTokenNode : IJackTokenNode
    {
        #region Fields

        private readonly IJackTokenizer jackTokenizer;

        #endregion

        #region Constructors

        public JackTokenNode(IJackTokenizer jackTokenizer)
        {
            this.jackTokenizer = jackTokenizer;
            this.GetNextToken();
            this.AdvanceToken();
        }

        #endregion

        #region Properties

        public bool ExistsNextToken => this.jackTokenizer.HasMoreTokens();

        public IJackToken Current { get; private set; }

        public IJackToken Next { get; private set; } 

        #endregion

        #region Public Methods

        public void AdvanceToken()
        {
            this.Current = this.Next;

            if (this.ExistsNextToken)
            {
                this.GetNextToken();
            }
        }

        public bool IsCurrentTokenClassKeyword()
        {
            return JackTokenJudgmentService.IsTokenClassKeyword(this.Current);
        }

        public bool IsCurrentTokenClassVarDecKeyword()
        {
            return JackTokenJudgmentService.IsTokenClassVarDecKeyword(this.Current);
        }

        public bool IsCurrentTokenSubroutineDecKeyword()
        {
            return JackTokenJudgmentService.IsTokenSubroutineDecKeyword(this.Current);
        }

        public bool IsCurrentTokenTypeKeyword()
        {
            return JackTokenJudgmentService.IsTokenTypeKeyword(this.Current);
        }

        public bool IsCurrentTokenVarDecKeyword()
        {
            return JackTokenJudgmentService.IsTokenVarDecKeyword(this.Current);
        }

        public bool IsCurrentTokenFirstStatementWord()
        {
            return JackTokenJudgmentService.IsTokenInFirstStatementWord(this.Current);
        }

        public bool IsCurrentTokenOperation()
        {
            return JackTokenJudgmentService.IsTokenOperation(this.Current);
        }

        public bool IsCurrentTokenUnaryOperation()
        {
            return JackTokenJudgmentService.IsTokenUnaryOperation(this.Current);
        }

        public bool IsCurrentTokenKeywordConstant()
        {
            return JackTokenJudgmentService.IsTokenKeywordConstant(this.Current);
        }

        #endregion

        #region Private Methods

        private void GetNextToken()
        {
            this.jackTokenizer.Advance();

            var tokenType = this.jackTokenizer.TokenType();

            switch (tokenType)
            {
                case JackTokenType.Keyword:
                    this.Next = new JackToken(tokenType, this.jackTokenizer.KeyWord());
                    break;

                case JackTokenType.Symbol:
                    this.Next = new JackToken(tokenType, this.jackTokenizer.Symbol());
                    break;

                case JackTokenType.IntegerConstant:
                    this.Next = new JackToken(tokenType, this.jackTokenizer.IntVal().ToString());
                    break;

                case JackTokenType.StringConstant:
                    this.Next = new JackToken(tokenType, this.jackTokenizer.StringVal());
                    break;

                case JackTokenType.Identifier:
                    this.Next = new JackToken(tokenType, this.jackTokenizer.Identifier());
                    break;

                case JackTokenType.Unknown:
                    Debug.Assert(false, "Found an unknown token");
                    break;

                default:
                    Debug.Assert(false, "Unexpected error");
                    break;
            }
        }


        #endregion
    }
}
