using System;
using System.Collections.Generic;
using System.Text;

namespace Serval.Fault
{
    public enum ErrorCodes
    {
        None                    = 0,

        // Lexing errors
        LexHangingEscape        = 1000,
        LexUnterminatedString   = 1001,
        LexInvalidConstant      = 1002,
        LexIdentifierTooLong    = 1003,
        LexInvalidHexChar       = 1004,
        LexUnknownEscape        = 1005,
        LexExpectedEndOfString  = 1006,
        LexExpectedEndOfChar    = 1007,
        LexBadHex               = 1008,
        LexBadBin               = 1009,

        LexUnknownError         = 1999,

        // Parsing errors
        ParseUnexpectedSymbol   = 2001,
        ParseExpectedSymbol     = 2002,
        ParseUnexpectedEOF      = 2003,
        ParseAlreadyDefined     = 2004,
        ParseUndeclaredVar      = 2005,
        ParseAssignToNonVar     = 2006,
        ParseTypeNotValidHere   = 2007,
        ParseTypeExpected       = 2008,
        ParseTypeUndefined      = 2009,

        ParseUnknownError       = 2999,
    }
}
