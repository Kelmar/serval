namespace Serval.Parsing.AST
{
    public class UnaryExpr : Expression
    {
        public UnaryExpr(char op, Expression rhs)
        {
            Operator = op;
            Right = rhs;
        }

        public char Operator { get; }

        public Expression Right { get; }

        public override string ToString()
        {
            return $"[UNI {Operator}({Right})]";
        }
    }
}
