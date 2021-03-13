namespace JackCompiler.Interfaces
{
    public interface IJackToken
    {
        JackTokenType Type { get; }
        
        string Value { get; }
    }
}
