using Serval.CodeGen;
using Serval.Lexing;

namespace Serval.AST
{
    /// <summary>
    /// Type operator: typeof, sizeof
    /// </summary>
    public class TypeOpExpr : ExpressionNode
    {
        public TypeOpExpr(TokenType op, Symbol type)
        {
            Operator = op;
            Type = type;
        }

        public TokenType Operator { get; }

        public Symbol Type { get; }
    }
}
