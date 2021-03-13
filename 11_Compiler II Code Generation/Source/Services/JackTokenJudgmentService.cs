using System.Text.RegularExpressions;

using JackCompiler.Interfaces;

namespace JackCompiler.Services
{
    public static class JackTokenJudgmentService
    {
        public static bool IsTokenClassKeyword(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.ClassKeywordPattern);
        }

        public static bool IsTokenClassVarDecKeyword(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.ClassVarDecKeywordPattern);
        }

        public static bool IsTokenSubroutineDecKeyword(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.SubroutineDecKeywordPattern);
        }

        public static bool IsTokenTypeKeyword(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.TypeKeywordPattern);
        }

        public static bool IsTokenVarDecKeyword(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.VarDeclarationKeywordPattern);
        }

        public static bool IsTokenInFirstStatementWord(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.StatementFirstKeywordPattern);
        }

        public static bool IsTokenOperation(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.OperatorsPattern);
        }

        public static bool IsTokenUnaryOperation(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.UnaryOperatorsPattern);
        }

        public static bool IsTokenKeywordConstant(IJackToken token)
        {
            return Regex.IsMatch(token.Value, JackCompileEngineConstants.KeywordConstantsPattern);
        }
    }
}
