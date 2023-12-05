using System;

namespace Serval.Lexing
{
    public class Token
    {
        public Token(string literal, TokenType type, int lineNumber, int start, int end)
        {
            Literal = literal;
            Parsed = null;
            Type = type;
            LineNumber = lineNumber;
            Start = start;
            End = end;
        }

        public static Token Error(int lineNumber, int start)
        {
            return new Token(String.Empty, TokenType.Error, lineNumber, start, -1);
        }

        /// <summary>
        /// Start position of token in line.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// End position of token in line.
        /// </summary>
        public int End { get; }

        /// <summary>
        /// The literal string of the token found
        /// </summary>
        public string Literal { get; }

        /// <summary>
        /// Holds the value of constant expressions.
        /// </summary>
        public object Parsed { get; init; }

        /// <summary>
        /// The type of token we've found
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// The line number we saw this token on
        /// </summary>
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
