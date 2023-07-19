using System;

using Serval.Lexing;

namespace Serval.Parsing.AST
{
    public class VariableExpr : Expression
    {
        public VariableExpr(Token token)
        {
            Token = token;
        }

        public Token Token { get; }

        //public TypeExpr ResultType => throw new NotImplementedException();
    }
}
