using System;
using System.Collections.Generic;
using System.Text;

namespace Serval.AST
{
    public abstract class BodyExpression : ExpressionNode
    {
        public List<StatementNode> Statements { get; } = new List<StatementNode>();
    }
}
