using Serval.CodeGen;

namespace Serval.AST
{
    public class AssignmentStatement : StatementNode
    {
        public AssignmentStatement(Symbol target, ExpressionNode expression)
        {
            Target = target;
            Expression = expression;
        }

        //public StatementNode Target { get; }
        public Symbol Target { get; }

        public ExpressionNode Expression { get; }
    }
}
