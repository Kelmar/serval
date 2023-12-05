using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Serval.AST;
using Serval.Fault;

namespace Serval.CodeGen
{
    public class Generator : IDisposable
    {
        //private readonly IDictionary<string, DeclarationExpr> m_symbolTable = new Dictionary<string, DeclarationExpr>();

        public Generator()
        {
        }

        public void Dispose()
        {
        }

        public void Generate(Module module)
        {
            Debug.Assert(module != null, "Got NULL AST list!?");

            foreach (var expr in module.Expressions)
            {
                //var value = Generate(expr);
            }
        }
    }
}
