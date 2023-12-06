using Serval.Lexing;

namespace Serval.AST
{
    public class PrimaryExpr : ExpressionNode
    {
        public PrimaryExpr(Token token)
        {
            Token = token;
            //ResultType = resultType;
        }

        public Token Token { get; }

        //public TypeExpr ResultType { get; }

        public override string ToString()
        {
            return $"{Token.Type}({Token.Literal})";
        }
    }
}
