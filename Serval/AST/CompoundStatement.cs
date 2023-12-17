using System.Collections.Generic;

namespace Serval.AST
{
    public class CompoundStatement : StatementNode
    {
        public List<StatementNode> Statements { get; } = new List<StatementNode>();
    }
}
