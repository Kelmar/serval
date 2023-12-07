namespace Serval.CodeGen
{
    public enum SymbolType
    {
        /// <summary>
        /// Symbol is declared as a variable
        /// </summary>
        Variable  = 1,

        /// <summary>
        /// Symbol was declared as a label (immutable)
        /// </summary>
        Label     = 2,

        /// <summary>
        /// Symbol was declared as a type (immutable)
        /// </summary>
        Type      = 3
    }
}
