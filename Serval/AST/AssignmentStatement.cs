using Serval.Lexing;

namespace Serval.AST
{
    public class AssignmentStatement : StatementExpr
    {
        public AssignmentStatement(Token ident, ExpressionNode body)
        {
            Identifier = ident;
            Body = body;
        }

        public Token Identifier { get; }

        public ExpressionNode Body { get; }

        public override string ToString()
        {
            return $"{Identifier.Literal} = {Body}";
        }
    }
}
