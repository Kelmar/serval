using System;
using System.Collections.Generic;
using System.Text;

using Serval.Parsing.AST;

namespace Serval
{
    public class Symbol
    {
        public Symbol(VariableDecl decl)
        {
            Declaration = decl;
        }

        public string Name => Declaration.Identifier.Literal;

        public VariableDecl Declaration { get; set; }
    }
}
