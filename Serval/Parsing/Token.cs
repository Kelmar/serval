namespace LangTest
{
    public class Token
    {
        public Token(string literal, TokenType type, int lineNumber)
        {
            Literal = literal;
            Type = type;
            LineNumber = lineNumber;
        }

        public string Literal { get; }

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
