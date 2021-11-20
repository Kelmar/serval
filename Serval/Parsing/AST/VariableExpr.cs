using Serval.Lexing;

namespace Serval.Parsing.AST
{
    public class VariableExpr : Expression
    {
        public VariableExpr(Token token, DeclarationExpr toDecl)
        {
            Token = token;
            ToDecl = toDecl;
        }

        public Token Token { get; }

        public TypeExpr ResultType => ToDecl.Type;

        public DeclarationExpr ToDecl { get; }
    }
}
