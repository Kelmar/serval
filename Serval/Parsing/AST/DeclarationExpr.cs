using Serval.Lexing;

namespace Serval.Parsing.AST
{
    public class VariableDecl : Expression
    {
        public VariableDecl(TokenType modifier, TokenType type, Token ident)
        {
            Modifier = modifier;
            Type = type;
            Identifier = ident;
        }

        public TokenType Modifier { get; }

        public Token Identifier { get; }

        public TokenType Type { get; }

        public override string ToString()
        {
            return $"[DECL {Type} {Identifier.Literal}]";
        }
    }
}
