using System;
using System.Collections.Generic;
using System.Text;

namespace Serval.Parsing.AST
{
    public abstract class BodyExpression : Expression
    {
        public List<Expression> Expressions { get; } = new List<Expression>();
    }
}
