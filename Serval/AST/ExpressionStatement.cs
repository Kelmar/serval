namespace Serval.AST
{
    public class ExpressionStatement : StatementNode
    {
        public ExpressionStatement(ExpressionNode expr)
        {
            Expression = expr;
        }

        public ExpressionNode Expression { get; }
    }
}
