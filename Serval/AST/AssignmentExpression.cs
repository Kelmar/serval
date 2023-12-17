namespace Serval.AST
{
    public class AssignmentExpression : ExpressionNode
    {
        public AssignmentExpression(ExpressionNode target, ExpressionNode expression)
        {
            Target = target;
            Expression = expression;
        }

        public ExpressionNode Target { get; }

        public ExpressionNode Expression { get; }
    }
}
