namespace Serval.CodeGen
{
    public class Symbol
    {
        public Symbol(string name, SymbolType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public SymbolType Type { get; }

        public int LineNumber { get; set; } = -1;
    }
}
