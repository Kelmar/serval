namespace Serval.CodeGen
{
    public class Symbol
    {
        public Symbol(string name, SymbolType type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// The name of this symbol.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type usage for this symbol.
        /// </summary>
        public SymbolType Type { get; }

        /// <summary>
        /// Set if this is an internally defined symbol by the compiler.
        /// </summary>
        public bool IsSpecial { get; set; } = false;

        /// <summary>
        /// The line number this symbol was declared on (0 for no line)
        /// </summary>
        public int LineNumber { get; set; } = 0;

        public override string ToString() => Name;
    }
}
