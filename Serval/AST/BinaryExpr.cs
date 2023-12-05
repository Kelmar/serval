namespace Serval.AST
{
    public class BinaryExpr : Expression
    {
        public BinaryExpr(string op, Expression lhs, Expression rhs)
        {
            Operator = op;
            Left = lhs;
            Right = rhs;
        }

        public string Operator { get; }

        public Expression Left { get; }

        public Expression Right { get; }

        public override string ToString()
        {
            return $"{Left} {Right} {Operator}";
        }
    }
}
