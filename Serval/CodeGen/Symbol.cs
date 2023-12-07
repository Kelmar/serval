using System;

namespace Serval.CodeGen
{
    public class Symbol
    {
        /// <summary>
        /// The name of this symbol.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// The type usage for this symbol.
        /// </summary>
        public SymbolType Type { get; init; }

        /// <summary>
        /// Set if this was an undefined symbol.
        /// </summary>
        public bool Undefined { get; init; }

        /// <summary>
        /// Set if this is an internally defined symbol by the compiler.
        /// </summary>
        public bool IsSpecial { get; init; } = false;

        /// <summary>
        /// The line number this symbol was declared on (0 for no line)
        /// </summary>
        public int LineNumber { get; init; } = 0;

        public override string ToString() => (Undefined ? "[UNDEFINED] " : String.Empty) + $"{Type} {Name}";
    }
}
