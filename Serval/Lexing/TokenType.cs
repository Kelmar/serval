namespace Serval.Lexing
{
    public enum TokenType
    {
        // Single character literals all occur in areas less than 256  (ASCII)

        EndOfFile       = 0x0000_00_05, // The lexer uses this as an end of stream token to the parser.

        // Basic types
        Identifier      = 0x0000_01_00,
        CharConst       = 0x0000_01_01,
        StringConst     = 0x0000_01_02,
        IntConst        = 0x0000_01_10, // 32-bit integer
        FloatConst      = 0x0000_01_20, // IEEE floating point

        // Keywords
        Discard         = 0x0000_11_00, // _

        For             = 0x0000_13_00,
        Var             = 0x0000_13_01,

        Enum            = 0x0000_14_00,
        Func            = 0x0000_14_01,

        Class           = 0x0000_15_00,
        Const           = 0x0000_15_01,
        While           = 0x0000_15_02,

        Import          = 0x0000_16_00,
        Public          = 0x0000_16_01,
        SizeOf          = 0x0000_16_02,
        Struct          = 0x0000_16_03,
        TypeOf          = 0x0000_16_04,

        Private         = 0x0000_17_00,

        Protected       = 0x0000_19_00,

        // Misc symbols
        At              = '@',
        Dot             = '.',
        Comma           = ',',
        Colon           = ':',
        Semicolon       = ';',
        LeftParen       = '(',
        RightParen      = ')',
        Question        = '?',
        Assign          = '=',
        LeftBracket     = '[',
        RightBracket    = ']',
        LeftCurl        = '{',
        RightCurl       = '}',

        // Operator symbols (Grouped roughly by prec)
        LogicalNot      = '!', // Unary
        Not             = '~', // Unary

        Mul             = '*', // Also unary (pointer deref)
        Div             = '/',
        Mod             = '%',

        Add             = '+', // Also unary
        Sub             = '-', // Also unary

        Less            = '<',
        More            = '>',

        And             = '&',

        Xor             = '^',

        Or              = '|',

        Increment       = 0x0000_22_00, // ++
        Decrement       = 0x0000_22_01, // --
        ShiftLeft       = 0x0000_22_02, // <<
        LessEqual       = 0x0000_22_03, // <=
        ShiftRight      = 0x0000_22_04, // >>
        MoreEqual       = 0x0000_22_05, // >=
        LogicalAnd      = 0x0000_22_06, // &&
        LogicalOr       = 0x0000_22_07, // ||
        Equals          = 0x0000_22_08, // ==
        NotEqual        = 0x0000_22_09, // !=

        Arrow           = 0x0000_22_0A, // ->

        AddAssign       = 0x0000_32_00, // +=
        SubAssign       = 0x0000_32_01, // -=
        AndAssign       = 0x0000_32_02, // &=
        OrAssign        = 0x0000_32_03, // |=
        MulAssign       = 0x0000_32_04, // *=
        DivAssign       = 0x0000_32_05, // /=
        ModAssign       = 0x0000_32_06, // %=
        XorAssign       = 0x0000_32_07, // ^=
        Range           = 0x0000_32_08, // ..

        LeftAssign      = 0x0000_33_00, // <<=
        RightAssign     = 0x0000_33_01, // >>=
        SpaceShip       = 0x0000_33_02, // <=>

        Spread          = 0x0000_33_03, // ...

        // Comment symbols
        EolComment      = 0x0000_42_00, // //
        CommentStart    = 0x0000_42_01, // /*
        CommentEnd      = 0x0000_42_02, // */

        /// <summary>
        /// Lexing error token.
        /// </summary>
        Error           = 0x7FFF_FF_FF
    }
}
