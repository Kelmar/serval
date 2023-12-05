using System;
using Serval.Lexing;

namespace Serval.AST
{
    internal class ConstExpr : Expression
    {
        public ConstExpr(Token token)
        {
            Token = token;

            Type = token.Type switch
            {
                TokenType.CharConst => TokenType.Char,
                TokenType.StringConst => TokenType.String,
                TokenType.IntConst => TokenType.Int,
                TokenType.FloatConst => TokenType.Float,
                _ => throw new Exception($"Unknown constant type {token.Type}")
            };
        }

        public Token Token { get; }

        public TokenType Type { get; }
    }
}
