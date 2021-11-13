using System;
using System.Linq;
using System.Text;

namespace Serval
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
     * \r      -- Carrage return
     * \t      -- Tab
     * \v      -- Vertical Tab
     * \000    -- Octal escaped ASCII character
     * \x hh   -- Hexidecimal ASCII character
     * \x hhhh -- Hexidecimal UTF-16 character
     */
    public partial class Lexer
    {
        private class ControlEscape
        {
            public char Character { get; set; }

            public char Control { get; set; }
        }

        private string literalEscapes = "\"'\\";

        private ControlEscape[] controlEscapes = new ControlEscape[]
        {
            new ControlEscape{ Character = 'a', Control = '\a' },
            new ControlEscape{ Character = 'b', Control = '\b' },
            new ControlEscape{ Character = 'f', Control = '\f' },
            new ControlEscape{ Character = 'n', Control = '\n' },
            new ControlEscape{ Character = 'r', Control = '\r' },
            new ControlEscape{ Character = 't', Control = '\t' },
            new ControlEscape{ Character = 'v', Control = '\v' }
        };

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

        private int ConvertHexChar(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            else if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;
            else if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;

            return -1;
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
                Error("Invalid hexidecimal escape string.");
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
                    Error("Start of escape character encountered at end of line.");
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
                    byte[] b = new byte[1] { (byte)val };
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
                        byte[] b = new byte[1] { (byte)val };
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
                        Error("Unknown escape character '{0}'", CurrentChar);
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

            literal.Append('"');

            ++m_linePos; // Eat opening double quote

            while (m_linePos < m_line.Length && CurrentChar != '"')
            {
                (string p, char c) = ReadSingleChar();

                if (p == null)
                    return null;

                literal.Append(p);
                parsed.Append(c);
            }

            if (CurrentChar != '"')
            {
                Error("Unexpected '{0}' character, expecting \"", CurrentChar);
                return null;
            }

            literal.Append('"');
            ++m_linePos; // Eat closing double quote

            return new Token(literal.ToString(), parsed.ToString(), TokenType.StringConst, m_lineNumber);
        }

        private Token ReadChar()
        {
            string literal = "'";
            ++m_linePos; // Eat opening single quote

            (string parsed, char c) = ReadSingleChar();

            if (parsed == null)
                return null;

            literal += parsed;

            if (CurrentChar != '\'')
            {
                Error("Unexpected '{0}' character, expecting '", CurrentChar);
                return null;
            }

            literal += "'";
            ++m_linePos; // Eat closing single quote

            return new Token(literal, c.ToString(), TokenType.CharConst, m_lineNumber);
        }
    }
}
