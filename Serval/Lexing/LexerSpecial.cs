﻿namespace Serval.Lexing
{
    public partial class Lexer
    {
        private Token ReadSpecial()
        {
            string literal = CurrentChar.ToString();
            TokenType type = (TokenType)CurrentChar;
            int start = m_linePos;

            ++m_linePos;

            if (m_linePos < m_line.Length)
            {
                switch (literal)
                {
                case "+":
                    if (CurrentChar == '+')
                    {
                        literal = "++";
                        type = TokenType.Increment;
                        ++m_linePos;
                    }
                    else if (CurrentChar == '=')
                    {
                        literal = "+=";
                        type = TokenType.AddAssign;
                        ++m_linePos;
                    }
                    break;

                case "-":
                    switch (CurrentChar)
                    {
                    case '-':
                        literal = "--";
                        type = TokenType.Decrement;
                        ++m_linePos;
                        break;

                    case '=':
                        literal = "-=";
                        type = TokenType.SubAssign;
                        ++m_linePos;
                        break;

                    case '>':
                        literal = "->";
                        type = TokenType.Arrow;
                        ++m_linePos;
                        break;
                    }
                    
                    break;

                case "<":
                    if (CurrentChar == '<')
                    {
                        literal = "<<";
                        type = TokenType.ShiftLeft;
                        ++m_linePos;

                        if (m_linePos < m_line.Length && CurrentChar == '=')
                        {
                            literal = "<<=";
                            type = TokenType.LeftAssign;
                            ++m_linePos;
                        }
                    }
                    else if (CurrentChar == '=')
                    {
                        literal = "<=";
                        type = TokenType.LessEqual;
                        ++m_linePos;

                        if (CurrentChar == '>')
                        {
                            literal = "<=>";
                            type = TokenType.SpaceShip;
                            ++m_linePos;
                        }
                    }
                    break;

                case ">":
                    if (CurrentChar == '>')
                    {
                        literal = ">>";
                        type = TokenType.ShiftRight;
                        ++m_linePos;

                        if (m_linePos < m_line.Length && CurrentChar == '=')
                        {
                            literal = ">>=";
                            type = TokenType.RightAssign;
                            ++m_linePos;
                        }
                    }
                    else if (CurrentChar == '=')
                    {
                        literal = ">=";
                        type = TokenType.MoreEqual;
                        ++m_linePos;
                    }
                    break;

                case "&":
                    if (CurrentChar == '&')
                    {
                        literal = "&&";
                        type = TokenType.LogicalAnd;
                        ++m_linePos;
                    }
                    else if (CurrentChar == '=')
                    {
                        literal = "&=";
                        type = TokenType.AndAssign;
                        ++m_linePos;
                    }
                    break;

                case "|":
                    if (CurrentChar == '|')
                    {
                        literal = "||";
                        type = TokenType.LogicalOr;
                        ++m_linePos;
                    }
                    else if (CurrentChar == '=')
                    {
                        literal = "|=";
                        type = TokenType.OrAssign;
                        ++m_linePos;
                    }
                    break;

                case "=":
                    if (CurrentChar == '=')
                    {
                        literal = "==";
                        type = TokenType.Equals;
                        ++m_linePos;
                    }
                    break;

                case "!":
                    if (CurrentChar == '=')
                    {
                        literal = "!=";
                        type = TokenType.NotEqual;
                        ++m_linePos;
                    }
                    break;

                case "*":
                    if (CurrentChar == '=')
                    {
                        literal = "*=";
                        type = TokenType.MulAssign;
                        ++m_linePos;
                    }
                    else if (CurrentChar == '/')
                    {
                        /**
                         * This is really just a place holder; we have to scan for the end of a block comment differently.
                         * E.g. The following is a legitamit comment, which would break with using this symbol parser.
                         */
                        // /* **/
                        
                        literal = "*/";
                        type = TokenType.CommentEnd;
                        ++m_linePos;
                    }
                    break;

                case "/":
                    if (CurrentChar == '=')
                    {
                        literal = "/=";
                        type = TokenType.DivAssign;
                        ++m_linePos;
                    }
                    else if (CurrentChar == '/')
                    {
                        literal = "//";
                        type = TokenType.EolComment;
                        ++m_linePos;
                    }
                    else if (CurrentChar == '*')
                    {
                        literal = "/*";
                        type = TokenType.CommentStart;
                        ++m_linePos;
                    }
                    break;

                case "%":
                    if (CurrentChar == '=')
                    {
                        literal = "%=";
                        type = TokenType.ModAssign;
                        ++m_linePos;
                    }
                    break;

                case "^":
                    if (CurrentChar == '=')
                    {
                        literal = "^=";
                        type = TokenType.XorAssign;
                        ++m_linePos;
                    }
                    break;

                case ".":
                    if (CurrentChar == '.')
                    {
                        literal = "..";
                        type = TokenType.Range;
                        ++m_linePos;

                        if (CurrentChar == '.')
                        {
                            literal = "...";
                            type = TokenType.Spread;
                            ++m_linePos;
                        }
                    }
                    break;
                }
            }

            var rval = new Token(literal, type, m_lineNumber, start, m_linePos);

            return rval;
        }
    }
}
