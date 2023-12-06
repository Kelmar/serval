namespace Serval.AST
{
    public class BinaryExpr : ExpressionNode
    {
        public BinaryExpr(string op, ExpressionNode lhs, ExpressionNode rhs)
        {
            Operator = op;
            Left = lhs;
            Right = rhs;
        }

        public string Operator { get; }

        public ExpressionNode Left { get; }

        public ExpressionNode Right { get; }

        public override string ToString()
        {
            return $"{Left} {Right} {Operator}";
        }
    }
}
