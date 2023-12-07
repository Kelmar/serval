using Serval.CodeGen;

namespace Serval.AST
{
    public class AssignmentStatement : StatementExpr
    {
        public AssignmentStatement(Symbol ident, ExpressionNode body)
        {
            Identifier = ident;
            Body = body;
        }

        public Symbol Identifier { get; }

        public ExpressionNode Body { get; }

        public override string ToString()
        {
            return $"{Identifier.Name} = {Body}";
        }
    }
}
