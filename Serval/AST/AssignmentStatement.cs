using Serval.CodeGen;

namespace Serval.AST
{
    public class AssignmentStatement : StatementExpr
    {
        public AssignmentStatement(Symbol ident, ExpressionNode expression)
        {
            Identifier = ident;
            Expression = expression;
        }

        public Symbol Identifier { get; }

        public ExpressionNode Expression { get; }

        public override string ToString()
        {
            return $"{Identifier.Name} = {Expression}";
        }
    }
}
