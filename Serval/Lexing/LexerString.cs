using System;
using System.Linq;
using System.Text;

using Serval.Fault;

namespace Serval.Lexing
{
    /*
     * Parses out strings and characters.  This allows for escaped characters that are similar to the C# rules.
     * 
     * The following escapes should be recognized:
     * \"      -- Literal Double Quote
     * \'      -- Literal Single Quote
     * \\      -- Literal backslash
     * \a      -- Bell character
     * \b      -- Backspace character
     * \f      -- Form feed
     * \n      -- New line
     * \r      -- Carriage return
     * \t      -- Tab
     * \v      -- Vertical Tab
     * \000    -- Octal escaped ASCII character
     * \x hh   -- Hexadecimal ASCII character
     * \x hhhh -- Hexadecimal UTF-16 character
     */
    public partial class Lexer
    {
        private class ControlEscape
        {
            public char Character { get; set; }

            public char Control { get; set; }
        }

        private readonly string literalEscapes = "\"'\\";

        private readonly ControlEscape[] controlEscapes =
        [
            new() { Character = 'a', Control = '\a' },
            new() { Character = 'b', Control = '\b' },
            new() { Character = 'f', Control = '\f' },
            new() { Character = 'n', Control = '\n' },
            new() { Character = 'r', Control = '\r' },
            new() { Character = 't', Control = '\t' },
            new() { Character = 'v', Control = '\v' }
        ];

        private (string literal, int value) ReadStringOctal()
        {
            string literal = String.Empty;
            int value = 0;
            int cnt = 0;

            while (m_linePos < m_line.Length)
            {
                if (cnt == 3)
                    break;

                if (CurrentChar < '0' || CurrentChar > '7')
                    break;

                value = value * 8 + (CurrentChar - '0');

                literal += CurrentChar;
                ++m_linePos;
                ++cnt;
            }

            return (literal, value);
        }

        private (string literal, int value) ReadStringHex()
        {
            string literal = CurrentChar.ToString();
            int value = 0;
            int cnt = 0;

            ++m_linePos; // Eat the 'x'

            while (m_linePos < m_line.Length && cnt < 4)
            {
                int cVal = ConvertHexChar(CurrentChar);

                if (cVal < 0)
                    break;

                value = value * 16 + cVal;
                literal += CurrentChar;
                ++m_linePos;
                ++cnt;
            }

            if (cnt == 0 || cnt == 1)
            {
                Error(ErrorCodes.LexBadHex);
                return (null, '\0');
            }

            if (cnt == 3)
            {
                // Back up one can only have 2 or 4 digits.
                --m_linePos;
                value /= 16;
            }

            return (literal, value);
        }

        private (string literal, char c) ReadSingleChar()
        {
            string literal = CurrentChar.ToString();
            char rval = CurrentChar;
            ++m_linePos;

            if (literal == "\\")
            {
                if (m_linePos >= m_line.Length)
                {
                    Error(ErrorCodes.LexHangingEscape);
                    return (null, '\0');
                }

                if (literalEscapes.Contains(CurrentChar))
                {
                    literal += CurrentChar;
                    rval = CurrentChar;
                    ++m_linePos;
                }
                else if (CurrentChar == '0')
                {
                    // Octal escaped character (up to 3 digits)
                    (string oct, int val) = ReadStringOctal();
                    literal += oct;
                    byte[] b = [(byte)val];
                    rval = Encoding.ASCII.GetString(b)[0];
                }
                else if (CurrentChar == 'x')
                {
                    // Hex escaped character (2 or 4 digits)
                    (string hex, int val) = ReadStringHex();

                    if (hex == null)
                        return (null, '\0');

                    literal += hex;

                    if (hex.Length == 2)
                    {
                        byte[] b = [(byte)val];
                        rval = Encoding.ASCII.GetString(b)[0];
                    }
                    else
                    {
                        byte[] b = BitConverter.GetBytes((ushort)val);
                        rval = Encoding.Unicode.GetString(b)[0];
                    }
                }
                else
                {
                    var ctl = controlEscapes.FirstOrDefault(ce => ce.Character == CurrentChar);

                    if (ctl != null)
                    {
                        literal += CurrentChar;
                        rval = ctl.Control;
                        ++m_linePos;
                    }
                    else
                    {
                        Error(ErrorCodes.LexUnknownEscape, CurrentChar);
                        return (null, '\0');
                    }
                }
            }

            return (literal, rval);
        }

        private Token ReadString()
        {
            var literal = new StringBuilder();
            var parsed = new StringBuilder();
            int start = m_linePos;

            literal.Append('"');

            ++m_linePos; // Eat opening double quote

            while (m_linePos < m_line.Length && CurrentChar != '"')
            {
                (string p, char c) = ReadSingleChar();

                if (p == null)
                    return GetErrorToken();

                literal.Append(p);
                parsed.Append(c);
            }

            if (CurrentChar != '"')
            {
                Error(ErrorCodes.LexExpectedEndOfString);
                return GetErrorToken();
            }

            literal.Append('"');
            ++m_linePos; // Eat closing double quote

            return new Token(literal.ToString(), TokenType.StringConst, m_lineNumber, start, m_linePos)
            {
                Parsed = parsed.ToString()
            };
        }

        private Token ReadChar()
        {
            string literal = "'";
            int start = m_linePos;

            ++m_linePos; // Eat opening single quote

            (string parsed, char c) = ReadSingleChar();

            if (parsed == null)
                return GetErrorToken();

            literal += parsed;

            if (CurrentChar != '\'')
            {
                Error(ErrorCodes.LexExpectedEndOfChar);
                return GetErrorToken();
            }

            literal += "'";
            ++m_linePos; // Eat closing single quote

            return new Token(literal, TokenType.CharConst, m_lineNumber, start, m_linePos)
            {
                Parsed = c.ToString() // TODO: Change to char type later.
            };
        }
    }
}
