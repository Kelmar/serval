using System;
using System.Collections.Generic;
using System.Text;

namespace Serval.AST
{
    public class FunctionCallExpr : StatementNode
    {
        public FunctionCallExpr(string name, List<StatementNode> arguments)
        {
            FunctionName = name;
            Arguments = arguments;
        }

        public string FunctionName { get; }

        public List<StatementNode> Arguments { get; }
    }
}
