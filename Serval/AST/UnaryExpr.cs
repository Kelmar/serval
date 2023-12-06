namespace Serval.AST
{
    public class UnaryExpr : ExpressionNode
    {
        public UnaryExpr(char op, ExpressionNode rhs)
        {
            Operator = op;
            Right = rhs;
        }

        public char Operator { get; }

        public ExpressionNode Right { get; }

        public override string ToString()
        {
            return $"[UNI {Operator}({Right})]";
        }
    }
}
