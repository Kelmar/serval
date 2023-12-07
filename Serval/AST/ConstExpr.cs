using System;

using Serval.CodeGen;
using Serval.Lexing;

namespace Serval.AST
{
    internal class ConstExpr : ExpressionNode
    {
        public ConstExpr(Token token, Symbol type)
        {
            Token = token;
            Type = type;
        }

        public Token Token { get; }

        public Symbol Type { get; }
    }
}
