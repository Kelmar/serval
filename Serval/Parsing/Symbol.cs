using System;
using System.Collections.Generic;
using System.Text;

using Serval.Parsing.AST;

namespace Serval
{
    public class Symbol
    {
        public Symbol(DeclarationExpr decl)
        {
            Declaration = decl;
        }

        public string Name => Declaration.Identifier.Literal;

        public DeclarationExpr Declaration { get; set; }
    }
}
