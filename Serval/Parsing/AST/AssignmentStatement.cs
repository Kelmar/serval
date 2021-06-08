namespace LangTest.Parsing.AST
{
    public class AssignmentStatement : StatementExpr
    {
        public AssignmentStatement(Token ident, Expression body)
        {
            Identifier = ident;
            Body = body;
        }

        public Token Identifier { get; }

        public Expression Body { get; }

        public override string ToString()
        {
            return $"{Identifier.Literal} = {Body}";
        }
    }
}
