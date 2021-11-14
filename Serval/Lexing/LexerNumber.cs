using System;

namespace Serval.Lexing
{
    public partial class Lexer
    {
        private string ReadNumberLiteral() => ReadWhile(c => c == '_' || Char.IsDigit(c));

        private Token ReadHex()
        {
            // Parse a hexidecimal number
            string number = "0";
            ++m_linePos;

            number += m_line[m_linePos];
            ++m_linePos;

            number += ReadWhile(c => Char.IsDigit(c) || c == '_' || ((c >= 'a') && (c <= 'f')) || ((c >= 'A') && (c <= 'F')));

            if (number.ToLower() != "0x")
                return new Token(number, TokenType.IntConst, m_lineNumber);

            Error("Invalid hexidecimal number on line {0}", m_lineNumber);
            return null;
        }

        private Token ReadNumber()
        {
            string number = "";

            // Check for hex
            if (CurrentChar == '0' && (m_linePos + 1) < m_line.Length && (m_line[m_linePos + 1] == 'x' || m_line[m_linePos + 1] == 'X'))
                return ReadHex();

            TokenType type = TokenType.IntConst;

            number += ReadNumberLiteral();

            // Check to see if we have a digit after the period
            if (CurrentChar == '.' && (m_linePos + 1) < m_line.Length && Char.IsDigit(m_line[m_linePos + 1]))
            {
                ++m_linePos;
                number += "." + ReadNumberLiteral();

                type = TokenType.FloatConst;
            }

            // Check to see if we have a digit after the 'E'
            if (CurrentChar == 'e' || CurrentChar == 'E' && (m_linePos + 1) < m_line.Length && Char.IsDigit(m_line[m_linePos + 1]))
            {
                number += CurrentChar;
                ++m_linePos;
                number += ReadNumberLiteral();

                type = TokenType.FloatConst;
            }

            // TODO: Add support of number type suffix.  E.g. 'L', 'UL', etc.

            if (!String.IsNullOrWhiteSpace(number))
                return new Token(number, type, m_lineNumber);

            return null;
        }
    }
}
