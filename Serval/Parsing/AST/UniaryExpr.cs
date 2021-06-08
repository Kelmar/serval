using System;
using System.Collections.Generic;
using System.Text;

namespace LangTest.Parsing.AST
{
    public class UniaryExpr : Expression
    {
        public UniaryExpr(char op, Expression rhs)
        {
            Operator = op;
            Right = rhs;
        }

        public char Operator { get; }

        public Expression Right { get; }

        public override string ToString()
        {
            return $"[UNI {Operator}({Right})]";
        }
    }
}
