using Serval.CodeGen;

namespace Serval.AST
{
    class CastExpr : ExpressionNode
    {
        public CastExpr(Symbol type, ExpressionNode rhs)
        {
            Type = type;
            Right = rhs;
        }

        public Symbol Type { get; }

        public ExpressionNode Right { get; }

        public override string ToString()
        {
            return $"CAST[({Type}){Right}]";
        }
    }
}
