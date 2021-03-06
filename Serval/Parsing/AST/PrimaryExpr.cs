using Serval.Lexing;

namespace Serval.Parsing.AST
{
    public class PrimaryExpr : Expression
    {
        public PrimaryExpr(Token token, TypeExpr resultType)
        {
            Token = token;
            ResultType = resultType;
        }

        public Token Token { get; }

        public TypeExpr ResultType { get; }

        public override string ToString()
        {
            return $"{Token.Type}({Token.Literal})";
        }
    }
}
