using Serval.CodeGen;
using Serval.Lexing;

namespace Serval.AST
{
    public class VariableDecl : ExpressionNode
    {
        public VariableDecl(TokenType modifier, Symbol type, Symbol ident)
        {
            Modifier = modifier;
            Type = type;
            Identifier = ident;
        }

        public TokenType Modifier { get; }

        public Symbol Identifier { get; }

        public Symbol Type { get; }

        public override string ToString()
        {
            return $"[DECL {Type.Name} {Identifier.Name}]";
        }
    }
}
