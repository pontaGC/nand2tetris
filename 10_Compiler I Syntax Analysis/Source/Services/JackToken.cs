using JackCompiler.Interfaces;

namespace JackCompiler.Services
{
    internal class JackToken : IJackToken
    {
        public JackToken(JackTokenType type, string tokenValue)
        {
            this.Type = type;
            this.Value = tokenValue;
        }

        public JackTokenType Type { get; }

        public string Value { get; }
    }
}
