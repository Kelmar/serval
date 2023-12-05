using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Serval.AST;
using Serval.Fault;

namespace Serval
{
    public class SymbolTable
    {
        private readonly IDictionary<string, Symbol> m_entries = new Dictionary<string, Symbol>();

        public Symbol AddEntry(VariableDecl decl)
        {
            if (m_entries.ContainsKey(decl.Identifier.Literal))
                throw new CompileException(CompileErrors.SymbolRedefined, String.Empty, 0, null);

            var symbol = new Symbol(decl);

            m_entries[symbol.Name] = symbol;

            return symbol;
        }

        public Symbol FindEntry(string name)
        {
            if (m_entries.TryGetValue(name, out Symbol rval))
                return rval;

            return null;
        }
    }
}
