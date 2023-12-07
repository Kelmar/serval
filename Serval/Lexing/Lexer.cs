using Serval.Fault;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Serval.Lexing
{    
    public partial class Lexer : IDisposable, IEnumerator<Token>
    {
        private readonly StreamReader m_input;
        private readonly IReporter m_reporter;

        private string m_line;
        private int m_lineNumber;
        private int m_linePos;

        public Lexer(Stream input, IReporter reporter)
        {
            Debug.Assert(input != null);
            Debug.Assert(reporter != null);
            Debug.Assert(input.CanRead);

            m_input = new StreamReader(input);
            m_reporter = reporter;

            m_line = "";
            m_lineNumber = 0;
            m_linePos = 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                m_input.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Token Current { get; private set; }

        public Token LookAhead { get; private set; }

        private char CurrentChar => m_line != null && m_linePos < m_line.Length ? m_line[m_linePos] : '\0';

        private char NextChar => m_line != null && (m_linePos + 1) < m_line.Length ? m_line[m_linePos + 1] : '\0';

        object IEnumerator.Current => Current;

        private Token GetErrorToken() => Token.Error(m_lineNumber, m_linePos);

        private void Error(ErrorCodes errorCode, params object[] args)
        {
            m_reporter.Error(m_lineNumber, errorCode, args);
        }

        private bool ReadLine()
        {
            m_line = m_input.ReadLine();

            if (m_line == null)
                return false; 

            ++m_lineNumber;
            m_linePos = 0;

            return true;
        }

        private string ReadWhile(Func<char, bool> predicate, bool spanLines = false)
        {
            // If we start at the end of a line, go ahead and read a new line regardless of spanLines.
            if (m_line != null && m_linePos >= m_line.Length)
            {
                if (!ReadLine())
                    return null;
            }

            StringBuilder sb = new StringBuilder();

            int start = m_linePos;

            while (m_line != null)
            {
                if (m_linePos < m_line.Length)
                {
                    if (predicate(CurrentChar))
                        ++m_linePos;
                    else
                        break;
                }
                else
                {
                    if (spanLines)
                    {
                        sb.Append(m_line.Substring(start, m_linePos - start));
                        sb.Append('\n');

                        start = 0;

                        ReadLine();
                    }
                    else
                        break;
                }
            }

            if (start != m_linePos)
                sb.Append(m_line.Substring(start, m_linePos - start));

            return sb.ToString();
        }

        private void EatWhiteSpace() => ReadWhile(Char.IsWhiteSpace, true);

        private Token ReadTokenInner()
        {
            if (m_input.EndOfStream && (m_line == null || m_linePos >= m_line.Length))
                return new Token(String.Empty, TokenType.EndOfFile, m_lineNumber, -1, -1);

            EatWhiteSpace();

            if (m_line != null)
            {
                if (m_linePos >= m_line.Length)
                {
                    if (!ReadLine())
                        return null;
                }

                if (CurrentChar != '\0')
                {
                    if (Char.IsDigit(CurrentChar))
                        return ReadNumber();

                    if (Char.IsLetter(CurrentChar))
                        return ReadKeywordOrIdentifier();

                    //if (CurrentChar == '$' || CurrentChar == '"')
                    if (CurrentChar == '"')
                        return ReadString();

                    if (CurrentChar == '\'')
                        return ReadChar();

                    return ReadSpecial();
                }
            }

            return null;
        }

        private Token ReadSingleToken()
        {
            bool inBlockComment = false;

            for (;;)
            {
                if (inBlockComment)
                {
                    int idx = m_line.IndexOf("*/");

                    if (idx == -1)
                    {
                        if (!ReadLine())
                        {
                            // Reached end of file.
                            Error(ErrorCodes.ParseUnexpectedEOF);
                            return null;
                        }
                    }
                    else
                    {
                        m_linePos = idx + 2;
                        inBlockComment = false;
                    }
                }
                else
                {
                    Token rval = ReadTokenInner();

                    if (rval == null)
                        return null;

                    if (rval.Type == TokenType.CommentStart)
                    {
                        inBlockComment = true;
                        continue;
                    }

                    if (rval.Type == TokenType.EolComment)
                    {
                        if (!ReadLine())
                            return null; // End of file

                        continue;
                    }

                    return rval;
                }
            }
        }

        public bool MoveNext()
        {
            if (Current?.Type == TokenType.EndOfFile)
                return false;

            if (LookAhead == null)
                LookAhead = ReadSingleToken();

            Current = LookAhead;
            LookAhead = ReadSingleToken();

            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();

            //m_input.BaseStream.Seek(0, SeekOrigin.Begin);
            //Current = null;
        }

        public override string ToString()
        {
            return $"CUR: '{Current.Literal}'  NEXT: '{LookAhead.Literal}'";
        }
    }
}
