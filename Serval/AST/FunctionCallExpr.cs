using System;
using System.Collections.Generic;
using System.Text;

namespace Serval.AST
{
    public class FunctionCallExpr : ExpressionNode
    {
        public FunctionCallExpr(string name, List<ExpressionNode> arguments)
        {
            FunctionName = name;
            Arguments = arguments;
        }

        public string FunctionName { get; }

        public List<ExpressionNode> Arguments { get; }
    }
}
