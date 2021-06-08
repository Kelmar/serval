using System;

namespace Serval.Parsing.AST
{
    public class PrimaryExpr : Expression
    {
        public PrimaryExpr(Token token)
        {
            Token = token;
        }

        public Token Token { get; }

        public override string ToString()
        {
            return $"{Token.Type}({Token.Literal})";
        }
    }
}
