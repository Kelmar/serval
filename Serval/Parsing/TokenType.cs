namespace LangTest
{
    public enum TokenType
    {
        // Literals all occur in areas less than 256  (ASCII)
        EndOfFile   = 0x0000_00_05,

        // Basic types
        Identifier  = 0x0000_01_00,
        CharConst   = 0x0000_01_01,
        StringConst = 0x0000_01_02,
        IntConst    = 0x0000_01_10, // 32-bit integer
        FloatConst  = 0x0000_01_20, // IEEE floating point
        
        // Keywords
        For         = 0x0000_13_00,
        Int         = 0x0000_13_01,

        Class       = 0x0000_15_00,
        Const       = 0x0000_15_01,
        Float       = 0x0000_15_02,

        String      = 0x0000_16_00,
        Struct      = 0x0000_16_01,

        // Misc symbols
        Dot         = '.',
        Comma       = ',',
        Colon       = ':',
        Simicolon   = ';',
        LeftParen   = '(',
        RightParen  = ')',

        // Operator symbols (Groupped roughly by prec)
        LogicalNot  = '!', // Uniary
        Not         = '~', // Uniary

        Mul         = '*', // Also uniary (pointer deref)
        Div         = '/',
        Mod         = '%',

        Add         = '+', // Also uniary
        Sub         = '-', // Also uniary

        Less        = '<',
        More        = '>',

        And         = '&', // Also uniary (get pointer)

        Xor         = '^',

        Or          = '|',

        
        Increment   = 0x0000_22_00, // ++
        Decrement   = 0x0000_22_01, // --
        ShiftLeft   = 0x0000_22_02, // <<
        LessEqual   = 0x0000_22_03, // <=
        ShiftRight  = 0x0000_22_04, // >>
        MoreEqual   = 0x0000_22_05, // >=
        LogicalAnd  = 0x0000_22_06, // &&
        LogicalOr   = 0x0000_22_07, // ||
        Equals      = 0x0000_22_08, // ==
        NotEqual    = 0x0000_22_09, // !=

        // Assignment symbols
        Assign      = '=',

        AddAsign    = 0x0000_32_00, // +=
        SubAssign   = 0x0000_32_01, // -=
        AndAssign   = 0x0000_32_02, // &=
        OrAssign    = 0x0000_32_03, // |=
        MulAssign   = 0x0000_32_04, // *=
        DivAssign   = 0x0000_32_05, // /=
        ModAssign   = 0x0000_32_06, // %=
        XorAssign   = 0x0000_32_07, // ^=

        LeftAssign  = 0x0000_33_00, // <<=
        RightAssign = 0x0000_33_01, // >>=
    }
}
