using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serval.CodeGen
{
    public class TypeDecl
    {
        public TypeDecl(string name, SymbolTable parent)
        {
            Name = name;
            Members = new SymbolTable(parent);
        }

        public string Name { get; }

        public SymbolTable Members { get; }
    }
}
