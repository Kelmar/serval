namespace Serval.CodeGen
{
    public class TypeDecl
    {
        public TypeDecl(TypeType type, string name, SymbolTable parent)
        {
            Type = type;
            Name = name;
            Members = new SymbolTable(parent);
        }

        public TypeType Type { get; }

        public string Name { get; }

        public SymbolTable Members { get; }
    }
}
