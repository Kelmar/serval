namespace LangTest.Parsing.AST
{
    class CastExpr : Expression
    {
        public CastExpr(Token type, Expression rhs)
        {
            Type = type;
            Right = rhs;
        }

        public Token Type { get; }

        public Expression Right { get; }

        public override string ToString()
        {
            return $"CAST[({Type}){Right}]";
        }
    }
}
