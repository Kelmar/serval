using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Serval.Parsing.AST
{
    public class TypeExpr : Expression
    {
        public TypeExpr(Token type, IEnumerable<Token> modifiers)
        {
            Debug.Assert(type != null);

            Type = type;
            Modifiers = modifiers.ToList();
        }

        Token Type { get; }

        List<Token> Modifiers { get; }

        public override string ToString()
        {
            string rval = "";

            if (Modifiers.Any())
            {
                rval = String.Join(" ", Modifiers);
                rval += " ";
            }

            return rval + Type.Literal;
        }
    }
}
