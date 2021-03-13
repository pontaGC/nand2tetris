using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JackCompiler
{
    internal static class JackTokenConstants
    {
        #region Keyword

        internal static class Keyword
        {
            internal const string Class       = "class";
            internal const string Method      = "method";
            internal const string Function    = "function";
            internal const string Constructor = "constructor";

            internal const string Int     = "int";
            internal const string Boolean = "boolean";
            internal const string Char    = "char";
            internal const string Void    = "void";

            internal const string Var    = "var";
            internal const string Static = "static";
            internal const string Field  = "field";
            internal const string Let    = "let";

            internal const string Do     = "do";
            internal const string If     = "if";
            internal const string Else   = "else";
            internal const string While  = "while";
            internal const string Return = "return";

            internal const string True  = "true";
            internal const string False = "false";

            internal const string Null = "null";

            internal const string This = "this";
        }

        internal static IReadOnlyCollection<string> Keywords = new[]
        {
           Keyword.Class,
           Keyword.Method,
           Keyword.Function,
           Keyword.Constructor,

           Keyword.Int,
           Keyword.Boolean,
           Keyword.Char,
           Keyword.Void,

           Keyword.Var,
           Keyword.Static,
           Keyword.Field,
           Keyword.Let,

           Keyword.Do,
           Keyword.If,
           Keyword.Else,
           Keyword.While,
           Keyword.Return,

           Keyword.True,
           Keyword.False,

           Keyword.Null,

           Keyword.This,
        };

        internal static readonly string KeywordPattern = $"^{string.Join(@"|^", Keywords)}";
        internal static readonly Regex KeywordRegex = new Regex(KeywordPattern);

        #endregion

        #region Symbol

        internal static class Symbol
        {
            internal const string Period    = @".";
            internal const string Comma     = @",";
            internal const string SemiColon = @";";

            internal static readonly (string Left, string Right) RoundBracket  = (@"(", @")");
            internal static readonly (string Left, string Right) CurlyBracket  = (@"{", @"}");
            internal static readonly (string Left, string Right) SquareBracket = (@"[", @"]");

            internal const string Addition       = @"+";
            internal const string Subtraction    = @"-";
            internal const string Multiplication = @"*";
            internal const string Division       = @"/";
            internal const string And = @"&";
            internal const string Or  =  @"|";
            internal const string Not = @"~";

            internal const string Equal   = @"=";
            internal const string Less    = @"<";
            internal const string Greater = @">";

        }

        internal static IReadOnlyCollection<string> Symbols = new[] 
        {
                Symbol.Period,
                Symbol.Comma,
                Symbol.SemiColon,

                Symbol.RoundBracket.Left,
                Symbol.RoundBracket.Right,
                Symbol.CurlyBracket.Left,
                Symbol.CurlyBracket.Right,
                Symbol.SquareBracket.Left,
                Symbol.SquareBracket.Right,

                Symbol.Addition,
                Symbol.Subtraction,
                Symbol.Multiplication,
                Symbol.Division,

                Symbol.And,
                Symbol.Or,
                Symbol.Not,

                Symbol.Equal,
                Symbol.Less,
                Symbol.Greater,
        };

        internal static readonly string SymbolPattern = $"^{string.Join(@"|^", Symbols.Select(Regex.Escape))}";
        internal static readonly Regex SymbolRegex = new Regex(SymbolPattern);

        #endregion

        #region IntegerConstant

        internal static readonly (int Min, int Max) IntegerConstantRange = (0, 32767);

        internal static readonly string IntegerConstantPattern = "[0-9]+";
        internal static readonly Regex IntegerConstantRegex = new Regex($"^{IntegerConstantPattern}");

        #endregion

        #region StringConstant

        internal static readonly string StringConstantPattern = "\"(.*)\"";
        internal static readonly Regex StringConstantRegex = new Regex($"^\"(.*)\"");

        #endregion

        #region Identifier

        internal static readonly string IdentifierPattern = "[a-zA-Z_][a-zA-Z0-9_\\s]*";
        internal static readonly Regex IdentifierRegex = new Regex($"^{IdentifierPattern}");

        #endregion

        #region Tables

        internal static IReadOnlyDictionary<JackTokenType, Regex> TokenRegexTable = new Dictionary<JackTokenType, Regex>()
        {
            { JackTokenType.Identifier,      IdentifierRegex },
            { JackTokenType.Keyword,         KeywordRegex },
            { JackTokenType.Symbol,          SymbolRegex },
            { JackTokenType.IntegerConstant, IntegerConstantRegex },
            { JackTokenType.StringConstant,  StringConstantRegex },
        };

        #endregion

        #region For XML

        internal static IReadOnlyDictionary<JackTokenType, string>
            TokenTypeXmlTagNames = new Dictionary<JackTokenType, string>()
                                               {
                                                   { JackTokenType.Keyword,         @"keyword" },
                                                   { JackTokenType.Symbol,          @"symbol" },
                                                   { JackTokenType.Identifier,      @"identifier" },
                                                   { JackTokenType.IntegerConstant, @"integerConstant" },
                                                   { JackTokenType.StringConstant,  @"stringConstant" },
                                               };

        internal static IReadOnlyDictionary<string, string>
            XMlEscapeFormatTable = new Dictionary<string, string>()
                                                    {
                                                        { @"&", @"&amp;"},
                                                        { @"<", @"&lt;" },
                                                        { @">", @"&gt;" },
                                                    };


        #endregion

    }
}
