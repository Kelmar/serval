﻿using System;

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
        public SymbolUsage Usage { get; init; }

        /// <summary>
        /// The declared type of the symbol
        /// </summary>
        public Symbol Type { get; init; }

        /// <summary>
        /// Information about the declared type
        /// </summary>
        /// <remarks>
        /// Only valid if Usage == SymbolUsage.Type
        /// </remarks>
        public TypeDecl TypeDefinition { get; set; }

        /// <summary>
        /// Set if this was an undefined symbol.
        /// </summary>
        /// <remarks>
        /// We allow this to change for things like labels where a goto 
        /// might appear before the label is defined, which is valid.
        /// </remarks>
        public bool Undefined { get; set; } = false;

        /// <summary>
        /// Set if the symbol was validly defined.
        /// </summary>
        public bool Valid { get; set; } = true;

        /// <summary>
        /// Set if this is an internally defined symbol by the compiler.
        /// </summary>
        public bool IsSpecial { get; init; } = false;

        /// <summary>
        /// The line number this symbol was declared on (0 for no line)
        /// </summary>
        public int LineNumber { get; set; } = 0;

        /// <summary>
        /// Temporary for testing
        /// </summary>
        public object Value { get; set; }

        public override string ToString() => (Undefined ? "[UNDEFINED] " : String.Empty) + $"{Usage} '{Name}'";
    }
}
