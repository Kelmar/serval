using System.Collections.Generic;

using Serval.Lexing;

namespace Serval.CodeGen
{
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> m_entries = [];

        public Symbol AddEntry(string name, SymbolType type)
        {
            if (!m_entries.ContainsKey(name))
                m_entries[name] = new Symbol(name, type);
            
            return m_entries[name];
        }

        public Symbol AddEntry(Token decl, SymbolType type) => AddEntry(decl.Literal, type);
        
        public Symbol FindEntry(string name)
        {
            if (m_entries.TryGetValue(name, out Symbol rval))
                return rval;

            return null;
        }

        /// <summary>
        /// Initializes system built in entries for global symbol table.
        /// </summary>
        public void InitGlobal()
        {
            AddEntry("int", SymbolType.Type);
            AddEntry("char", SymbolType.Type);
            AddEntry("float", SymbolType.Type);
            AddEntry("string", SymbolType.Type);
        }
    }
}
