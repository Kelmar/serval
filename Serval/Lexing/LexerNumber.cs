using System;
using System.Diagnostics;
using System.Text;

using Serval.Fault;

namespace Serval.Lexing
{
    public partial class Lexer
    {
        private (string literal, string digits) ReadNumberLiteral()
        {
            var literal = new StringBuilder();
            var digits = new StringBuilder();

            while (m_linePos < m_line.Length)
            {
                if (CurrentChar == '_')
                {
                    literal.Append(CurrentChar);
                    break;
                }

                if (!Char.IsDigit(CurrentChar))
                    break;

                literal.Append(CurrentChar);
                digits.Append(CurrentChar);
                ++m_linePos;
            }

            return (literal.ToString(), digits.ToString());
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

        private Token ReadHex()
        {
            // Parse a hexadecimal number
            string literal = "0";
            int value = 0;
            int start = m_linePos;
            ++m_linePos;

            literal += m_line[m_linePos];
            ++m_linePos;

            while (m_linePos < m_line.Length)
            {
                if (CurrentChar == '_')
                {
                    literal += CurrentChar;
                    continue; // Skip it
                }

                int i = ConvertHexChar(CurrentChar);

                if (i == -1)
                    break;

                literal += CurrentChar;

                ++m_linePos;
                value = value * 16 + i;
            }

            if (literal.ToLower() != "0x")
            {
                return new Token(literal, TokenType.IntConst, m_lineNumber, start, m_linePos)
                {
                    Parsed = value
                };
            }

            Error(ErrorCodes.LexBadHex);
            return GetErrorToken();
        }

        private Token ReadNumber()
        {
            // Check for hex
            if (CurrentChar == '0' && (m_linePos + 1) < m_line.Length && (m_line[m_linePos + 1] == 'x' || m_line[m_linePos + 1] == 'X'))
                return ReadHex();

            int start = m_linePos;
            TokenType type = TokenType.IntConst;

            var literal = new StringBuilder();
            var digits = new StringBuilder();

            string l, d;
            
            (l, d) = ReadNumberLiteral();

            literal.Append(l);
            digits.Append(d);

            // Check to see if we have a digit after the period
            if (CurrentChar == '.' && (m_linePos + 1) < m_line.Length && Char.IsDigit(m_line[m_linePos + 1]))
            {
                ++m_linePos;

                (l, d) = ReadNumberLiteral();

                literal.Append('.');
                literal.Append(literal);

                digits.Append('.');
                digits.Append(d);

                type = TokenType.FloatConst;
            }

            // Check to see if we have a digit after the 'E'
            if (CurrentChar == 'e' || CurrentChar == 'E')
            {
                char litE = CurrentChar;
                string litSign = String.Empty;

                int numPos = m_linePos + 1;

                if (NextChar == '+' || NextChar == '-')
                {
                    litSign = NextChar.ToString();
                    ++numPos;
                }

                if ((m_linePos + numPos) < m_line.Length && Char.IsDigit(m_line[m_linePos + numPos]))
                {
                    m_linePos += numPos;

                    (l, d) = ReadNumberLiteral();

                    literal.Append(litE);
                    literal.Append(litSign);
                    literal.Append(l);

                    digits.Append(litE);
                    digits.Append(litSign);
                    digits.Append(d);

                    type = TokenType.FloatConst;
                }
            }

            Debug.Assert(literal.Length > 0, "BUG: Did not parse a valid number.");

            l = literal.ToString();
            d = digits.ToString();

            if (type == TokenType.FloatConst)
            {
                if (Double.TryParse(d, out double dbl))
                {
                    return new Token(l, TokenType.FloatConst, m_lineNumber, start, m_linePos)
                    {
                        Parsed = dbl
                    };
                }
            }
            else
            {
                if (Int32.TryParse(d, out int i))
                {
                    return new Token(l, TokenType.IntConst, m_lineNumber, start, m_linePos)
                    {
                        Parsed = i
                    };
                }
            }

            return GetErrorToken();
        }
    }
}
