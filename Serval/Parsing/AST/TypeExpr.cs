using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Serval.Lexing;

namespace Serval.Parsing.AST
{
    public class TypeExpr : Expression
    {
        public TypeExpr(Token type, IEnumerable<Token> modifiers = null)
        {
            Debug.Assert(type != null);

            Type = type;

            IsConstant = false;

            switch (type.Type)
            {
            case TokenType.IntConst:
            case TokenType.StringConst:
            case TokenType.FloatConst:
                IsConstant = true;
                break;
            }

            if (modifiers != null)
            {
                IsConstant = modifiers.Any(t => t.Type == TokenType.Const);
            }
        }

        Token Type { get; }

        public bool IsConstant { get; }

        public override string ToString()
        {
            List<string> modifiers = new List<string>();

            if (IsConstant)
                modifiers.Add("const");

            string rval = "";

            if (modifiers.Any())
            {
                rval = String.Join(" ", modifiers);
                rval += " ";
            }

            return rval + Type.Literal;
        }
    }
}
