using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JackCompiler
{
    internal static class JackCompileEngineConstants
    {
            public static readonly IReadOnlyCollection<string> ClassKeywords = new[]
            {
                JackTokenConstants.Keyword.Class,
            };

            public static readonly string ClassKeywordPattern = CreatePattern(ClassKeywords);

            public static readonly IReadOnlyCollection<string> ClassVarDeclarationKeywords = new[] 
            {
                JackTokenConstants.Keyword.Static,
                JackTokenConstants.Keyword.Field,
            };

            public static readonly string ClassVarDecKeywordPattern = CreatePattern(ClassVarDeclarationKeywords);

            public static readonly IReadOnlyCollection<string> TypeKeywords = new []
            {
                JackTokenConstants.Keyword.Int,
                JackTokenConstants.Keyword.Char,
                JackTokenConstants.Keyword.Boolean,
            };

            public static readonly string TypeKeywordPattern = CreatePattern(TypeKeywords);

            public static readonly IReadOnlyCollection<string> SubroutineDeclarationKeywords = new[]
            {
                JackTokenConstants.Keyword.Constructor,
                JackTokenConstants.Keyword.Function,
                JackTokenConstants.Keyword.Method,
            };

            public static readonly string SubroutineDecKeywordPattern = CreatePattern(SubroutineDeclarationKeywords);

            public static readonly IReadOnlyCollection<string> VarDeclarationKeywords = new[]
            {
                JackTokenConstants.Keyword.Var,
            };

            public static readonly string VarDeclarationKeywordPattern = CreatePattern(VarDeclarationKeywords);

            public static readonly IReadOnlyCollection<string> Operators = new[]
            {
                Regex.Escape(JackTokenConstants.Symbol.Addition),
                JackTokenConstants.Symbol.Subtraction,
                Regex.Escape(JackTokenConstants.Symbol.Multiplication),
                JackTokenConstants.Symbol.Division,

                JackTokenConstants.Symbol.And,
                Regex.Escape(JackTokenConstants.Symbol.Or),

                JackTokenConstants.Symbol.Equal,
                JackTokenConstants.Symbol.Less,
                JackTokenConstants.Symbol.Greater,
            };

            public static readonly string OperatorsPattern = CreatePattern(Operators);

            public static readonly IReadOnlyCollection<string> UnaryOperators = new[]
            {
                JackTokenConstants.Symbol.Subtraction,
                JackTokenConstants.Symbol.Not,
            };

            public static readonly string UnaryOperatorsPattern = CreatePattern(UnaryOperators);

            public static readonly IReadOnlyCollection<string> KeywordConstants = new[]
            {
                JackTokenConstants.Keyword.True,
                JackTokenConstants.Keyword.False,
                JackTokenConstants.Keyword.Null,
                JackTokenConstants.Keyword.This,
            };

            public static readonly string KeywordConstantsPattern = CreatePattern(KeywordConstants);

            public static readonly IReadOnlyDictionary<JackSyntaxType, string> StatementTables
            = new Dictionary<JackSyntaxType, string>()
            {
                { JackSyntaxType.Do,     JackTokenConstants.Keyword.Do },
                { JackSyntaxType.Let,    JackTokenConstants.Keyword.Let },
                { JackSyntaxType.If,     JackTokenConstants.Keyword.If },
                { JackSyntaxType.While,  JackTokenConstants.Keyword.While },
                { JackSyntaxType.Return, JackTokenConstants.Keyword.Return },
            };

            public static readonly string StatementFirstKeywordPattern = CreatePattern(StatementTables.Values.ToArray());

        #region For XML

        internal static readonly IReadOnlyDictionary<JackSyntaxType, string> SyntaxXmlTagNames
            = new Dictionary<JackSyntaxType, string>()
            {
                    { JackSyntaxType.Class,           @"class" },
                    { JackSyntaxType.ClassVarDec,     @"classVarDec" },
                    { JackSyntaxType.SubroutineDec,   @"subroutineDec" },
                    { JackSyntaxType.SubroutineBody,  @"subroutineBody" },
                    { JackSyntaxType.ParameterList,   @"parameterList" },
                    { JackSyntaxType.VarDec,          @"varDec" },

                    { JackSyntaxType.Statements,      @"statements" },
                    { JackSyntaxType.Do,              @"doStatement" },
                    { JackSyntaxType.Let,             @"letStatement" },
                    { JackSyntaxType.While,           @"whileStatement" },
                    { JackSyntaxType.Return,          @"returnStatement" },
                    { JackSyntaxType.If,              @"ifStatement" },

                    { JackSyntaxType.ExpressionList,  @"expressionList" },
                    { JackSyntaxType.Expression,      @"expression" },
                    { JackSyntaxType.Term,            @"term" },
            };


        #endregion

        #region Private Methods

        private static string CreatePattern(IReadOnlyCollection<string> targetTokenValues)
        {
            return $"^{string.Join(@"|^", targetTokenValues)}";
        }


        #endregion
    }
}
