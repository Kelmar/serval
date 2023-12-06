using Serval.Lexing;

namespace Serval.AST
{
    class CastExpr : ExpressionNode
    {
        public CastExpr(Token type, ExpressionNode rhs)
        {
            Type = type;
            Right = rhs;
        }

        public Token Type { get; }

        public ExpressionNode Right { get; }

        public override string ToString()
        {
            return $"CAST[({Type}){Right}]";
        }
    }
}
