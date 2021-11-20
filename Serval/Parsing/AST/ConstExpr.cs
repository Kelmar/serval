using Serval.Lexing;

namespace Serval.Parsing.AST
{
    internal class ConstExpr : Expression
    {
        public ConstExpr(Token token)
        {
            Token = token;

            ResultType = new TypeExpr(Token);
        }

        public Token Token { get; }

        public TypeExpr ResultType { get; }
    }
}
