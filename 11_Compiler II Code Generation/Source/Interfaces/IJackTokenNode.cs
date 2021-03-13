namespace JackCompiler.Interfaces
{
    public interface IJackTokenNode
    {
        #region Properties

        bool ExistsNextToken { get; }

        IJackToken Current { get; }

        IJackToken Next { get; }

        #endregion

        #region Methods

        void AdvanceToken();

        bool IsCurrentTokenClassKeyword();

        bool IsCurrentTokenClassVarDecKeyword();

        bool IsCurrentTokenSubroutineDecKeyword();

        bool IsCurrentTokenTypeKeyword();

        bool IsCurrentTokenVarDecKeyword();

        bool IsCurrentTokenFirstStatementWord();

        bool IsCurrentTokenOperation();

        bool IsCurrentTokenUnaryOperation();

        bool IsCurrentTokenKeywordConstant();

        #endregion
    }
}
