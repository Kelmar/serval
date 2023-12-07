using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public Symbol Add(Symbol symbol)
        {
            Debug.Assert(symbol != null);
            Debug.Assert(!String.IsNullOrWhiteSpace(symbol.Name));

            if (!m_entries.ContainsKey(symbol.Name))
                m_entries[symbol.Name] = symbol;

            return m_entries[symbol.Name];
        }

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
            Add(new Symbol
            {
                Name = "int",
                Type = SymbolType.Type,
                IsSpecial = true,
                LineNumber = 0
            });

            Add(new Symbol
            {
                Name = "char",
                Type = SymbolType.Type,
                IsSpecial = true,
                LineNumber = 0
            });

            Add(new Symbol
            {
                Name = "float",
                Type = SymbolType.Type,
                IsSpecial = true,
                LineNumber = 0
            });

            Add(new Symbol
            {
                Name = "string",
                Type = SymbolType.Type,
                IsSpecial = true,
                LineNumber = 0
            });
        }
    }
}
