using Serval.CodeGen;
using Serval.Lexing;

namespace Serval.AST
{
    public class TypeExpr : ExpressionNode
    {
        public TypeExpr(TokenType op, Symbol type)
        {
            Operator = op;
            Type = type;
        }

        public TokenType Operator { get; }

        public Symbol Type { get; }
    }
}
