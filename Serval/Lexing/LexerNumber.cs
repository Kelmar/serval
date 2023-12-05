using System;

namespace Serval.Lexing
{
    public partial class Lexer
    {
        private (string literal, int value) ReadNumberLiteral()
        {
            string literal = "";
            int value = 0;

            while (m_linePos < m_line.Length)
            {
                if (CurrentChar == '_')
                {
                    literal += CurrentChar;
                    break;
                }

                if (!Char.IsDigit(CurrentChar))
                    break;

                literal += CurrentChar;
                value = value * 10 + (CurrentChar - '0');
                ++m_linePos;
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

        private Token ReadHex()
        {
            // Parse a hexadecimal number
            string literal = "0";
            int value = 0;
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
                return new Token(literal, value, TokenType.IntConst, m_lineNumber);

            Error("Invalid hexadecimal number on line {0}", m_lineNumber);
            return null;
        }

        private Token ReadNumber()
        {
            string number = "";

            // Check for hex
            if (CurrentChar == '0' && (m_linePos + 1) < m_line.Length && (m_line[m_linePos + 1] == 'x' || m_line[m_linePos + 1] == 'X'))
                return ReadHex();

            TokenType type = TokenType.IntConst;

            int whole = 0, @decimal = 0, exp = 0;
            string parsed;
            
            (parsed, whole) = ReadNumberLiteral();

            number += parsed;

            // Check to see if we have a digit after the period
            if (CurrentChar == '.' && (m_linePos + 1) < m_line.Length && Char.IsDigit(m_line[m_linePos + 1]))
            {
                ++m_linePos;
                (parsed, @decimal) = ReadNumberLiteral();

                number += "." + parsed;

                type = TokenType.FloatConst;
            }

            // Check to see if we have a digit after the 'E'
            if (CurrentChar == 'e' || CurrentChar == 'E' && (m_linePos + 1) < m_line.Length && Char.IsDigit(m_line[m_linePos + 1]))
            {
                number += CurrentChar;
                ++m_linePos;

                (parsed, exp) = ReadNumberLiteral();

                number += parsed;

                type = TokenType.FloatConst;
            }

            // TODO: Add support of number type suffix.  E.g. 'L', 'UL', etc.

            if (String.IsNullOrWhiteSpace(number))
                return null;

            if (type == TokenType.FloatConst)
            {
                // Take each part and calculate the value.  There's probably a better way to do this. -- B.Simonds (Nov 13, 2021)
                double res = @decimal;

                res /= Math.Pow(10, @decimal == 0 ? 1 : (Math.Log10(@decimal) + 1));
                res += whole;
                res *= Math.Pow(10, exp);

                return new Token(number, (float)res, TokenType.FloatConst, m_lineNumber);
            }

            return new Token(number, whole, type, m_lineNumber);
        }
    }
}
