namespace Serval.CodeGen
{
    public enum SymbolUsage
    {
        /// <summary>
        /// Symbol is declared as a variable
        /// </summary>
        Variable  = 1,

        /// <summary>
        /// Symbol is declared as a constant (immutable)
        /// </summary>
        Constant  = 2,

        /// <summary>
        /// Symbol was declared as a label (immutable)
        /// </summary>
        Label     = 3,

        /// <summary>
        /// Symbol was declared as a type (immutable)
        /// </summary>
        Type      = 4
    }
}
