using Serval.CodeGen;

namespace Serval.AST
{
    public class LabeledStatement : StatementNode
    {
        public LabeledStatement(Symbol symbol, StatementNode next)
        {
            Symbol = symbol;
            Next = next;
        }

        public Symbol Symbol { get; }

        public StatementNode Next { get; }
    }
}
