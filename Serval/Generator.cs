using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Serval.Fault;
using Serval.Parsing.AST;


using Serval.Lexing;

//using static Llvm.NET.Interop.Library;

namespace Serval
{
    public class Generator : IDisposable
    {
        public Generator()
        {
        }

        public void Dispose()
        {
        }

        public void Generate(List<Expression> astList)
        {
            Debug.Assert(astList != null, "Got NULL AST list!?");

            foreach (var ast in astList)
            {
            }
        }
    }
}
