using System.Collections.Generic;
using System.Linq;
using JackCompiler.Interfaces;

namespace JackCompiler
{
    internal class SymbolTable : ISymbolTable
    {
        #region Fields

        private static readonly IReadOnlyDictionary<string, AttributeType> StringAttributeTypes = new Dictionary<string, AttributeType>()
        {
            { JackTokenConstants.Keyword.Field,  AttributeType.Field },
            { JackTokenConstants.Keyword.Static, AttributeType.Static },
            { JackTokenConstants.Keyword.Var,    AttributeType.Var },
            { VmConstants.Segment.Argument,           AttributeType.Argument },
        };

        private readonly IDictionary<string, OneIdentifierElement> classScopeSymbolTable;
        private readonly IDictionary<string, OneIdentifierElement> subroutineScopeSymbolTable;

        #endregion

        #region Constructors

       internal SymbolTable()
       {
            this.classScopeSymbolTable = new Dictionary<string, OneIdentifierElement>();
            this.subroutineScopeSymbolTable = new Dictionary<string, OneIdentifierElement>();
       }

        #endregion

        #region Public methods

        public void StartSubroutine()
        {
            this.subroutineScopeSymbolTable.Clear();
        }

        public void Define(string name, string type, AttributeType kind)
        {
            if (IsAttributeClassScope(kind))
            {
                RegisterOneIdentifer(this.classScopeSymbolTable, name, type, kind);
                return;
            }

            if (IsAttributeSubroutineScope(kind))
            {
                RegisterOneIdentifer(this.subroutineScopeSymbolTable, name, type, kind);
                return;
            }
        }

        public int VarCount(AttributeType kind)
        {
            if (IsAttributeSubroutineScope(kind))
            {
                return GetVarCountEveryAttributeType(this.subroutineScopeSymbolTable, kind);
            }

            if (IsAttributeClassScope(kind))
            {
                return GetVarCountEveryAttributeType(this.classScopeSymbolTable, kind);
            }

            return 0;
        }

        public AttributeType KindOf(string name)
        {
            if (this.subroutineScopeSymbolTable.ContainsKey(name))
            {
                return this.subroutineScopeSymbolTable[name].Kind;
            }

            if (this.classScopeSymbolTable.ContainsKey(name))
            {
                return this.classScopeSymbolTable[name].Kind;
            }

            return AttributeType.None;
        }

        public string TypeOf(string name)
        {
            if (this.subroutineScopeSymbolTable.ContainsKey(name))
            {
                return this.subroutineScopeSymbolTable[name].Type;
            }

            if (this.classScopeSymbolTable.ContainsKey(name))
            {
                return this.classScopeSymbolTable[name].Type;
            }

            return null;
        }

        public int IndexOf(string name)
        {
            if (this.subroutineScopeSymbolTable.ContainsKey(name))
            {
                return this.subroutineScopeSymbolTable[name].Index;
            }

            if (this.classScopeSymbolTable.ContainsKey(name))
            {
                return this.classScopeSymbolTable[name].Index;
            }

            return 0;
        }

        // this method is not defined by the book.
        public AttributeType ConvertStringKindToAttributeType(string kind)
        {
            if (StringAttributeTypes.TryGetValue(kind, out var attributeType) == false)
            {
                return AttributeType.None;
            }

            return attributeType;
        }

        #endregion

        #region Private methods

        private static void RegisterOneIdentifer(IDictionary<string, OneIdentifierElement> targetSymbolTable, 
                                                 string name, 
                                                 string type, 
                                                 AttributeType kind)
        {
            if (targetSymbolTable.ContainsKey(name))
            {
                return;
            }

            var index = GetVarCountEveryAttributeType(targetSymbolTable, kind);
            targetSymbolTable.Add(name, new OneIdentifierElement(type, kind, index));
        }

        private static int GetVarCountEveryAttributeType(IDictionary<string, OneIdentifierElement> targetSymbolTable,
                                                         AttributeType kind)
        {
            return targetSymbolTable.Values.Count(t => t.Kind == kind);
        }

        private static bool IsAttributeClassScope(AttributeType kind)
        {
            switch (kind)
            {
                case AttributeType.Static:
                case AttributeType.Field:
                    return true;
            }

            return false;
        }

        private static bool IsAttributeSubroutineScope(AttributeType kind)
        {
            switch (kind)
            {
                case AttributeType.Argument:
                case AttributeType.Var:
                    return true;
            }

            return false;
        }

        #endregion

        #region Symbol table

        private struct OneIdentifierElement
        {
            internal OneIdentifierElement(string type, AttributeType kind, int index)
            {
                this.Type = type;
                this.Kind = kind;
                this.Index = index;
            }

            internal string Type { get; }

            internal AttributeType Kind { get; }

            internal int Index { get; }
        }

        #endregion
    }
}
