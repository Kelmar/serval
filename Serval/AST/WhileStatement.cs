namespace Serval.AST
{
    public class WhileStatement : StatementNode
    {
        public ExpressionNode Condition { get; set; }

        public StatementNode Body { get; set; }
    }
}
