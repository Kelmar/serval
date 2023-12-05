using System;
using System.Collections.Generic;
using System.Text;

namespace Serval.AST
{
    public class FunctionCallExpr : Expression
    {
        public FunctionCallExpr(string name, List<Expression> arguments)
        {
            FunctionName = name;
            Arguments = arguments;
        }

        public string FunctionName { get; }

        public List<Expression> Arguments { get; }
    }
}
