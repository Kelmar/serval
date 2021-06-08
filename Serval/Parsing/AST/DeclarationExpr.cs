namespace LangTest.Parsing.AST
{
    public class DeclarationExpr : Expression
    {
        public DeclarationExpr(TypeExpr type, Token ident)
        {
            Type = type;
            Identifier = ident;
        }

        public Token Identifier { get; }

        public TypeExpr Type { get; }

        public override string ToString()
        {
            return $"[DECL {Type} {Identifier.Literal}]";
        }
    }
}
