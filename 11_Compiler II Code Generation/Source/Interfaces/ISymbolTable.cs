namespace JackCompiler.Interfaces
{
    public enum AttributeType
    {
        None, 
        Static,
        Field,
        Argument,
        Var,
    }

    public interface ISymbolTable
    {
        #region Methods

        void StartSubroutine();

        void Define(string name, string type, AttributeType kind);

        int VarCount(AttributeType kind);

        AttributeType KindOf(string name);

        string TypeOf(string name);

        int IndexOf(string name);

        // this method is not defined by the book.
        AttributeType ConvertStringKindToAttributeType(string kind);

        #endregion
    }
}
