using System.Collections.Generic;

namespace JackCompiler
{
    internal static class VmConstants
    {
        #region Segments

        internal struct Segment
        {
            internal static readonly string Constant = "constant";
            internal static readonly string Argument = "argument";
            internal static readonly string Local    = "local";
            internal static readonly string Static   = "static";
            internal static readonly string This     = "this";
            internal static readonly string That     = "that";
            internal static readonly string Pointer  = "pointer";
            internal static readonly string Temp     = "temp";
        }

        internal static readonly IReadOnlyCollection<string> AllSegments = new[]
        {
            Segment.Constant,
            Segment.Argument,
            Segment.Local,
            Segment.Static,
            Segment.This,
            Segment.That,
            Segment.Pointer,
            Segment.Temp,
        };

        #endregion

        #region VM commands

        internal struct VmCommands
        {
            internal static readonly string Pop = "pop";
            internal static readonly string Push = "push";

            internal static readonly string Label = "label";
            internal static readonly string Goto = "goto";
            internal static readonly string If = "if-goto";

            internal static readonly string Function = "function";
            internal static readonly string Call = "call";
            internal static readonly string Return = "return";
        }

        internal static readonly IReadOnlyCollection<string> AllVmCommands = new[]
        {
            VmCommands.Pop,
            VmCommands.Push,

            VmCommands.Label,
            VmCommands.Goto,
            VmCommands.If,

            VmCommands.Function,
            VmCommands.Call,
            VmCommands.Return,
        };

        #endregion

        #region Arithmetics commands

        internal struct Arithmetic
        {
            internal static readonly string Add = "add";
            internal static readonly string Subtraction = "sub";
            internal static readonly string Negation = "neg";
            internal static readonly string Equal = "eq";
            internal static readonly string Greater = "gt";
            internal static readonly string Less = "lt";
            internal static readonly string And = "and";
            internal static readonly string Or = "or";
            internal static readonly string Not = "not";
        }

        internal static readonly IReadOnlyCollection<string> AllArithmetics = new[]
        {
            Arithmetic.Add,
            Arithmetic.Subtraction,
            Arithmetic.Negation,
            Arithmetic.Equal,
            Arithmetic.Greater,
            Arithmetic.Less,
            Arithmetic.And,
            Arithmetic.Or,
            Arithmetic.Not,
        };

        internal static readonly IReadOnlyDictionary<string, string> JackVmUnaryOperationTable = new Dictionary<string, string>()
        {
            { JackTokenConstants.Symbol.Subtraction, Arithmetic.Negation },
            { JackTokenConstants.Symbol.Not, Arithmetic.Not },
        };

        internal static readonly IReadOnlyDictionary<string, string> JackVmBinaryOperationTable = new Dictionary<string, string>()
        {
            { JackTokenConstants.Symbol.Addition, Arithmetic.Add },
            { JackTokenConstants.Symbol.Subtraction, Arithmetic.Subtraction },
            { JackTokenConstants.Symbol.Equal, Arithmetic.Equal },
            { JackTokenConstants.Symbol.Greater, Arithmetic.Greater },
            { JackTokenConstants.Symbol.Less, Arithmetic.Less },
            { JackTokenConstants.Symbol.And, Arithmetic.And },
            { JackTokenConstants.Symbol.Or, Arithmetic.Or },
        };

        #endregion

        #region Keyword Constants

        internal static readonly IReadOnlyDictionary<string, int> KeywordConstantsVMValues = new Dictionary<string, int>()
        {
            { JackTokenConstants.Keyword.True, 1 },
            { JackTokenConstants.Keyword.False, 0 },
            { JackTokenConstants.Keyword.Null, 0 },
        };

        #endregion
    }
}
