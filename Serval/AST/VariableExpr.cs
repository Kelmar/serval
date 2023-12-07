using Serval.CodeGen;

namespace Serval.AST
{
    public class VariableExpr : ExpressionNode
    {
        public VariableExpr(Symbol symbol)
        {
            Symbol = symbol;
        }

        public Symbol Symbol { get; }
    }
}
