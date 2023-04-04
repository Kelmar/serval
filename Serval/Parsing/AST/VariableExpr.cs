using System;

using Serval.Lexing;

namespace Serval.Parsing.AST
{
    public class VariableExpr : Expression
    {
        public VariableExpr(Token token, Symbol symbol)
        {
            Token = token;
            Symbol = symbol;
        }

        public Token Token { get; }

        public TypeExpr ResultType => throw new NotImplementedException();

        public Symbol Symbol { get; }
    }
}
