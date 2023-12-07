using System.Collections.Generic;

using Serval.Lexing;

namespace Serval.CodeGen
{
    public class SymbolTable
    {
        private readonly Dictionary<string, Symbol> m_entries = [];

        /// <summary>
        /// Add a new entry to the symbol table.
        /// </summary>
        /// <remarks>
        /// If the symbol already exists, then the existing symbol is returned instead.
        /// </remarks>
        /// <param name="name">The name of the symbol to add</param>
        /// <param name="type">The symbol's basic type</param>
        /// <returns>The newly created symbol.</returns>
        public Symbol Add(string name, SymbolType type)
        {
            if (!m_entries.ContainsKey(name))
                m_entries[name] = new Symbol(name, type);

            return m_entries[name];
        }

        public Symbol Add(Token decl, SymbolType type) => Add(decl.Literal, type);

        /// <summary>
        /// Look for a symbol table entry
        /// </summary>
        /// <param name="name">The name of the symbol to look for.</param>
        /// <returns>The symbol if found, null if not.</returns>
        public Symbol Find(string name)
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
            Symbol e;

            e = Add("int", SymbolType.Type);
            e.IsSpecial = true;

            e = Add("char", SymbolType.Type);
            e.IsSpecial = true;

            e = Add("float", SymbolType.Type);
            e.IsSpecial = true;

            e = Add("string", SymbolType.Type);
            e.IsSpecial = true;
        }
    }
}
