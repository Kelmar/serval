namespace Serval.Lexing
{
    public class Token
    {
        public Token(string literal, TokenType type, int lineNumber)
            : this(literal, literal, type, lineNumber)
        {
        }

        public Token(string literal, string parsed, TokenType type, int lineNumber)
        {
            Literal = literal;
            Parsed = parsed;
            Type = type;
            LineNumber = lineNumber;
        }

        public string Literal { get; }

        public string Parsed { get; }

        public TokenType Type { get; }

        public int LineNumber { get; }

        public override string ToString()
        {
            if ((int)Type < 256 && Type != TokenType.EndOfFile)
            {
                char c = (char)Type;
                return $"'{c}'";
            }

            return Type.ToString();
        }
    }
}
