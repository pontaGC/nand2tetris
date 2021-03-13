namespace JackCompiler.Interfaces
{
    public interface IJackTokenizer
    {
        bool HasMoreTokens();

        void Advance();

        JackTokenType TokenType();

        string KeyWord();

        string Symbol();

        string Identifier();

        int IntVal();

        string StringVal();
    }
}
